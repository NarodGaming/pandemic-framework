using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlkalineThunder.Pandemic.Gui;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Settings;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkalineThunder.Pandemic.Scenes
{
    /// <summary>
    /// Provides access to the core user interface API for the Socially Distant engine.
    /// </summary>
    [RequiresModule(typeof(SkinSystem))]
    [RequiresModule(typeof (SettingsService))]
    [RequiresModule(typeof(InputService))]
    public class SceneSystem : EngineModule, IGuiContext
    {
        private List<Scene> _sceneStack = new List<Scene>();
        private Dictionary<Type, MarkupAttachedProperty[]> _attachedProps = new Dictionary<Type, MarkupAttachedProperty[]>();
        private Dictionary<Type, MarkupPropertyBuilder> _builders = new Dictionary<Type, MarkupPropertyBuilder>();
        private Dictionary<Type, MarkupPropertyInfo[]> _props = new Dictionary<Type, MarkupPropertyInfo[]>();
        private Dictionary<string, Type> _markupElements = new Dictionary<string, Type>();
        private bool _showBoundingRects;
        private Control _focusedControl;
        private Control _focusCandidate;
        private Control _hoveredControl;
        private Control _mouseDownControl;
        private MouseButtonEventArgs _mouseDownEvent;
        private bool _showFps;
        
        SceneSystem IGuiContext.SceneSystem => this;

        public SkinSystem Skin
            => GetModule<SkinSystem>();
        
        private SettingsService Settings
            => GetModule<SettingsService>();

        private InputService InputService
            => GetModule<InputService>();

        /// <summary>
        /// Gets or sets a value indicating whether clipping is enabled during the rendering process.
        /// </summary>
        public bool EnableClipping { get; set; } = true;
        
        /// <summary>
        /// Gets a value representing the screen area in local units.
        /// </summary>
        public Rectangle BoundingBox => new Rectangle(
                0,
                0,
                (int) GameLoop.LocalWidth,
                (int) GameLoop.LocalHeight
            );
        
        /// <summary>
        /// Unloads the top-most scene of the given type.
        /// </summary>
        /// <typeparam name="T">The type of scene to unload.</typeparam>
        public void UnloadScene<T>() where T : Scene, new()
        {
            for (var i = _sceneStack.Count - 1; i >= 0; i--)
            {
                if (_sceneStack[i] is T)
                {
                    _sceneStack[i].Unload();
                    _sceneStack.RemoveAt(i);
                }
            }
        }
        
        /// <summary>
        /// Pops the currently active scene from the Scene Stack.
        /// </summary>
        /// <remarks>
        /// This function is used when you want to send the player back to
        /// the previous <see cref="Scene" />.  Pairing this with <see cref="PushScene{T}" />,
        /// it is possible to unload the current scene while keeping the previous scene and all
        /// of it's state loaded in the background.
        ///
        /// When a scene is popped from the stack, the scene's <see cref="Scene.OnUnload" /> method is
        /// called.  It is good practice to remove any GUI controls that were added to the screen by the
        /// unloading scene at this point.
        ///
        /// If this method is called with only one scene in the stack, then the game will render nothing.
        /// This may or may not be desirable.
        /// </remarks>
        public void PopScene()
        {
            if (_sceneStack.Count > 0)
            {
                var scene = _sceneStack.Last();
                scene.Unload();
                _sceneStack.Remove(scene);
            }
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> and overwrites the scene stack with it.
        /// </summary>
        /// <typeparam name="T">The type of the desired <see cref="Scene" /> class to load.  Must be non-abstract and contain a parameter-less constructor.</typeparam>
        /// <remarks>
        /// Unlike <see cref="PushScene{T}" />, this method will pop all existing scenes from the stack, causing them to unload.
        /// Therefore, it is not possible to use <see cref="PopScene" /> to return the player to the previous scene, as the game
        /// has no idea what that scene was.  This may or may not be desirable.
        /// </remarks>
        public void GoToScene<T>() where T : Scene, new()
        {
            while (_sceneStack.Count > 0) PopScene();
            PushScene<T>();
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/>, and replaces the current scene with it.
        /// </summary>
        /// <typeparam name="T">The type of the desired <see cref="Scene" /> class to load.  Must be non-abstract and have a parameter-less constructor.</typeparam>
        /// <remarks>
        /// The behaviour of this method is similar to <see cref="PushScene{T}" />, however it will first
        /// pop the currently active scene from the stack.  It is still possible to use <see cref="PopScene"/>,
        /// however the player will not be returned to the scene that was once active before this method was used.
        ///
        /// This is desirable if, for example, you want to send the player from a main menu or start screen
        /// to the main game scene, or from the game scene to a main menu or start screen.  This is because the previous
        /// scene will unload all of it's resources.
        ///
        /// If, however, you want to unload all scenes currently in the stack, it is recommended that you use
        /// <see cref="GoToScene{T}"/> instead. 
        /// </remarks>
        public void ReplaceScene<T>() where T : Scene, new()
        {
            PopScene();
            PushScene<T>();
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/>, pushing it to the scene stack.
        /// </summary>
        /// <typeparam name="T">The type of the desired <see cref="Scene"/> class to load.  Must be non-abstract and have a parameter-less constructor.</typeparam>
        public T PushScene<T>() where T : Scene, new()
        {
            var scene = LoadScene<T>();
            _sceneStack.Add(scene);
            return scene;
        }

        private T LoadScene<T>() where T : Scene, new()
        {
            var scene = new T();
            scene.Load(this);
            return scene;
        }
        
        internal MarkupAttachedProperty GetAttachedPropertyInfo(Type control, string name)
        {
            if (_attachedProps.ContainsKey(control))
                return _attachedProps[control].FirstOrDefault(x => x.Name == name);
            return null;
        }
        
        internal Type GetMarkupElementType(string name)
        {
            if (_markupElements.ContainsKey(name))
                return _markupElements[name];
            else
                return null;
        }
        
        /// <summary>
        /// Determines whether a control currently has keyboard focus.
        /// </summary>
        /// <param name="control">The control to check the focus state of.</param>
        /// <returns>A value indicating whether the control is in focus.</returns>
        public bool IsFocused(Control control)
        {
            return control == _focusedControl;
        }
        
        internal MemberInfo GetMarkupProperty(Type type, string name)
        {
            return _props[type].FirstOrDefault(x => x.Name == name)?.MemberInfo;
        }

        internal MarkupPropertyBuilder GetPropertyBuilder(Type type)
        {
            if (_builders.ContainsKey(type))
                return _builders[type];
            return null;
        }
        
        private void FindMarkupElements()
        {
            _markupElements.Clear();

            foreach (var builder in this.GetType().Assembly.GetTypes()
                .Where(x => x.InheritsFrom(typeof(MarkupPropertyBuilder))))
            {
                if (builder.GetConstructor(Type.EmptyTypes) != null)
                {
                    var attribute = builder.GetCustomAttributes(false).OfType<MarkupTypeAttribute>().FirstOrDefault();

                    if (attribute != null)
                    {
                        if (_builders.ContainsKey(attribute.Type))
                            continue;

                        _builders.Add(attribute.Type, (MarkupPropertyBuilder) Activator.CreateInstance(builder, null));
                    }
                }
            }
            
            foreach (var control in this.GetType().Assembly.GetTypes().Where(x => x.InheritsFrom(typeof(Control))))
            {
                if (control.GetConstructor(Type.EmptyTypes) != null)
                {
                    var attribute = control.GetCustomAttributes(false).OfType<MarkupElementAttribute>()
                        .FirstOrDefault();

                    if (attribute != null)
                    {
                        if (_markupElements.ContainsKey(attribute.Name))
                            continue;

                        var propInfo = new List<MarkupPropertyInfo>();
                        var attachedInfo = new List<MarkupAttachedProperty>();
                        
                        foreach (var field in control.GetFields(BindingFlags.Public | BindingFlags.Static)
                            .Where(x => x.FieldType == typeof(string)))
                        {
                            var name = field.GetValue(null).ToString();
                            var attachedAttribute = field.GetCustomAttributes(false).OfType<MarkupTypeAttribute>()
                                .FirstOrDefault();

                            if (attachedAttribute != null)
                            {
                                attachedInfo.Add(new MarkupAttachedProperty(attachedAttribute.Type, $"{attribute.Name}.{name}"));
                            }
                        }
                        
                        foreach (var property in control.GetProperties())
                        {
                            var propAttributee = property.GetCustomAttributes(false).OfType<MarkupPropertyAttribute>()
                                .FirstOrDefault();

                            if (propAttributee != null)
                            {
                                if (propInfo.Any(x => x.Name == propAttributee.Name))
                                    continue;
                                
                                propInfo.Add(new MarkupPropertyInfo(propAttributee.Name, property));
                            }
                        }
                        
                        _props.Add(control, propInfo.ToArray());
                        _markupElements.Add(attribute.Name, control);
                        _attachedProps.Add(control, attachedInfo.ToArray());
                    }
                }
            }
        }
        
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            FindMarkupElements();
            
            GameUtils.Log("Binding input events for GUI...");

            InputService.MouseMove += this.Input_MouseMove;
            InputService.MouseDown += this.Input_MouseDown;
            InputService.MouseUp += this.Input_MouseUp;
            InputService.MouseScroll += Input_MouseScroll;
            InputService.MouseDoubleClick += Input_MouseDoubleClick;
            
            InputService.KeyDown += Input_KeyDown;
            InputService.KeyUp += Input_KeyUp;
            InputService.TextInput += Input_Text;
        }
        
        
        /// <summary>
        /// Called by Socially Distant when it is time for the <see cref="SceneSystem"/> to load its initial content.
        /// Do not call this method manually.
        /// </summary>
        protected override void OnLoadContent()
        {
            this.Settings.SettingsUpdated += (sender, args) =>
            {
                ForceLayoutUpdate();
            };
        }

        private void UpdateLayout()
        {
            if (_sceneStack.Any())
            {
                var root = _sceneStack.Last().Gui;
                root.FixedWidth = BoundingBox.Width;
                root.FixedHeight = BoundingBox.Height;
                root.Layout(BoundingBox);
            }
        }

        /// <inheritdoc />
        protected override void OnUpdate(GameTime gameTime)
        {
            this.UpdateLayout();

            foreach (var scene in _sceneStack)
                scene.Update(gameTime);
        }

        /// <inheritdoc />
        protected override void OnDraw(GameTime gameTime, SpriteRocket2D renderer)
        {
            foreach (var scene in _sceneStack)
            {
                scene.Draw(gameTime, renderer);
            }
            
            // If the developer setting is enabled, draw a blue highlight over the hovered GUI element if any.
            if (Settings.HighlightHoveredGuiElement)
            {
                if (_hoveredControl != null)
                {
                    var bounds = _hoveredControl.BoundingBox;
                     
                    renderer.Begin();
                    renderer.FillRectangle(bounds, Color.Blue * 0.33f);
                    renderer.End();
                }
            }

            if (_showBoundingRects)
            {
                if (_sceneStack.Any())
                {
                    renderer.Begin();

                    var root = _sceneStack.Last().Gui;
                    
                    foreach (var child in root.CollapseControlTree())
                    {
                        renderer.DrawRectangle(child.ContentRectangle, Color.Yellow, 1);
                        renderer.DrawRectangle(child.BoundingBox, Color.White, 1);
                    }

                    renderer.End();
                }
            }


            if (_showFps)
            {
                var fps = 1f / gameTime.ElapsedGameTime.TotalSeconds;
                var fpsText = $"{Math.Round(fps)} fps";
                var font = Skin.GetFont(SkinFontStyle.Code);
                var m = font.MeasureString(fpsText);

                renderer.Begin();
                renderer.FillRectangle(new Rectangle(0, 0, (int) m.X, (int) m.Y), Color.Black);
                renderer.DrawString(font, fpsText, Vector2.Zero, Color.White);
                renderer.End();
            }
        }

        /// <summary>
        /// Console command (gui.fps) that sets whether the framerate counter is enabled.
        /// </summary>
        /// <param name="value">A value indicating whether the framerate counter should be drawn on-screen.</param>
        [Exec("gui.fps")]
        public void Exec_Fps(bool value)
        {
            _showFps = value;
        }

        /// <summary>
        /// Finds a <see cref="Control"/>  where the given coordinates are inside the control's bounds.
        /// </summary>
        /// <param name="x">The X coordinate of the control to find.</param>
        /// <param name="y">The Y coordinate of the control to find.</param>
        /// <returns>An instance of the <see cref="Control"/> that was found, or null.</returns>
        /// <seealso cref="Control.FindControl"/>
        public Control FindControl(int x, int y)
        {
            if (_sceneStack.Any())
            {
                var scene = _sceneStack.Last();
                return scene.Gui.FindControl(x, y);
            }

            return null;
        }

        /// <summary>
        /// Gives keyboard focus to the given <paramref name="control"/>.
        /// </summary>
        /// <param name="control">The instance of <see cref="Control"/> to receive keyboard focus.  Pass null to remove keyboard focus entirely.</param>
        public void SetFocus(Control control)
        {
            if (control != _focusedControl)
            {
                var e = new FocusEventArgs(_focusedControl, control);

                if (_focusedControl != null)
                {
                    Bubble(_focusedControl, x => x.InvokeLostFocus(e));
                }

                _focusedControl = control;

                if (_focusedControl != null)
                {
                    Bubble(_focusedControl, x => x.InvokeGainedFocus(e));
                }
            }
        }
        
        private void BubbleUntil(Control start, Control end, Func<Control, bool> predicate)
        {
            var p = start;
            while (p != null && p != end)
            {
                if (predicate(p))
                    return;
                p = p.Parent;
            }
        }
        
        private void Bubble(Control control, Func<Control, bool> predicate)
        {
            var p = control;
            while (p != null)
            {
                if (predicate(p))
                    return;
                p = p.Parent;
            }
        }
        
        private void Input_MouseMove(object sender, MouseMoveEventArgs e)
        {
            var hovered = this.FindControl(e.X, e.Y);

            if (hovered != _hoveredControl)
            {
                if (_hoveredControl != null)
                {
                    if (hovered == null || (hovered != null && !hovered.HasParent(_hoveredControl)))
                    {
                        Bubble(_hoveredControl, x => x.InvokeMouseLeave(e));
                    }
                }

                _hoveredControl = hovered;

                if (_hoveredControl != null)
                {
                    Bubble(_hoveredControl, x => x.InvokeMouseEnter(e));
                }
            }

            if (hovered != null)
            {
                Bubble(hovered, x => x.InvokeMouseMove(e));
            }
        }

        private void Input_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var hovered = this.FindControl(e.X, e.Y);

            // If we've hit a control before the mouse was released on a previous control, notify the previous control that the mouse was released on it.
            if (hovered != _mouseDownControl && _mouseDownControl != null)
            {
                BubbleUntil(_mouseDownControl, hovered, x => x.InvokeMouseUp(_mouseDownEvent));
            }

            _mouseDownControl = hovered;
            _mouseDownEvent = e;
            
            if (hovered != null)
            {
                if (e.Button == MouseButton.Left)
                {
                    _focusCandidate = hovered;
                }
                
                Bubble(hovered, x => x.InvokeMouseDown(e));
            }
        }

        private void Input_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var hovered = this.FindControl(e.X, e.Y);

            // If the control we've hit isn't the same control as the one hit by MouseDown, notify that control that
            // the mouse has been released.
            if (hovered != _mouseDownControl && _mouseDownControl != null)
            {
                BubbleUntil(_mouseDownControl, hovered, x => x.InvokeMouseUp(_mouseDownEvent));
            }
            // Otherwise, if we've hit the same control and our mouse button is the same, fire a click.
            else if (hovered != null && _mouseDownControl == hovered)
            {
                if (e.Button == _mouseDownEvent.Button)
                {
                    Bubble(hovered, x => x.InvokeClick(e));
                }
            }

            _mouseDownControl = null;
            _mouseDownEvent = null;
            
            if (hovered != null)
            {
                if (hovered == _focusCandidate && e.Button == MouseButton.Left)
                {
                    this.SetFocus(hovered);
                }

                Bubble(hovered, x => x.InvokeMouseUp(e));
            }
        }

        private void Input_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var hovered = this.FindControl(e.X, e.Y);

            // If the control we've hit isn't the same control as the one hit by MouseDown, notify that control that
            // the mouse has been released.
            if (hovered != _mouseDownControl && _mouseDownControl != null)
            {
                BubbleUntil(_mouseDownControl, hovered, x => x.InvokeMouseUp(_mouseDownEvent));
            }
            // Otherwise, if we've hit the same control and our mouse button is the same, fire a click.
            else if (hovered != null && _mouseDownControl == hovered)
            {
                if (e.Button == _mouseDownEvent.Button)
                {
                    Bubble(hovered, x => x.InvokeDoubleClick(e));
                }
            }

            _mouseDownControl = null;
            _mouseDownEvent = null;
            
            if (hovered != null)
            {
                if (hovered == _focusCandidate && e.Button == MouseButton.Left)
                {
                    this.SetFocus(hovered);
                }

                Bubble(hovered, x => x.InvokeMouseUp(e));
            }
        }

        
        private void Input_Text(object sender, KeyEventArgs e)
        {
            if (_focusedControl != null)
            {
                Bubble(_focusedControl, x => x.InvokeTextInput(e));
            }
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (_focusedControl != null)
            {
                Bubble(_focusedControl, x => x.InvokeKeyDown(e));
            }
        }

        private void Input_MouseScroll(object sender, MouseScrollEventArgs e)
        {
            if (_hoveredControl != null)
            {
                Bubble(_hoveredControl, x => x.InvokeMouseScroll(e));

                var hover = FindControl(e.X, e.Y);
                var mme = e.MouseMoveEvent;

                if (hover != _hoveredControl)
                {
                    if (hover == null || !hover.HasParent(_hoveredControl))
                    {
                        Bubble(_hoveredControl, x => x.InvokeMouseLeave(mme));
                    }

                }

                _hoveredControl = hover;

                if (_hoveredControl != null)
                {
                    Bubble(_hoveredControl, x => x.InvokeMouseEnter(mme));
                }
            }
        }
        
        private void Input_KeyUp(object sender, KeyEventArgs e)
        {
            if (_focusedControl != null)
            {
                Bubble(_focusedControl, x => x.InvokeKeyUp(e));
            }
        }
        
        /// <summary>
        /// Console command (gui.showBoundingRects) that enables or disables
        /// the GUI's bounding rectangle debugger.
        /// </summary>
        /// <param name="value">A value indicating whether the bounding rectangle debugger should be turned on.</param>
        [Exec("gui.showBoundingRects")]
        public void Exec_ShowBoundingRects(bool value)
        {
            _showBoundingRects = value;
        }
        
        /// <summary>
        /// Console command (gui.disableClipping) that enables or disables scissor clipping.
        /// </summary>
        /// <param name="value">A value indicating whether scissor clipping should be disabled.</param>
        [Exec("gui.disableClipping")]
        public void Exec_DisableClipping(bool value)
        {
            EnableClipping = !value;
        }

        /// <summary>
        /// Console command that sets the base resolution for the GUI coordinate system.
        /// </summary>
        /// <param name="w">The width of the GUI coordinate system.</param>
        /// <param name="h">The height of the GUI coordinate system.</param>
        [Exec("gui.setBaseResolution")]
        public void SetBaseResolution(int w, int h)
        {
            GameUtils.BaseResolution = new Vector2(w, h);
        }
        
        /// <summary>
        /// Console command (gui.darkMode) that enables or disables the skin's dark theme.
        /// </summary>
        /// <param name="value">A value indicating whether the dark theme should be used.</param>
        [Exec("gui.darkMode")]
        public void Exec_DarkMode(bool value)
        {
            Settings.EnableDarkTheme = value;
        }
        
        /// <summary>
        /// Forces all GUI elements to recalculate their layout information.
        /// </summary>
        [Exec("gui.forceLayoutUpdate")]
        public void ForceLayoutUpdate()
        {
            foreach (var scene in _sceneStack)
            foreach (var control in scene.Gui.CollapseControlTree())
                control.InvalidateMeasure();
        }
    }
}
