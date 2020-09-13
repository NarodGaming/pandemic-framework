using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Settings;

namespace AlkalineThunder.Pandemic.Input
{
    /// <summary>
    /// Provides the Pandemic Framework with a simple event-based input system.
    /// </summary>
    [RequiresModule(typeof(SettingsService))]
    public sealed class InputService : EngineModule
    {
        private MouseState _lastMouseState;
        private float _clickCooldown;
        private float _clickCooldownStart = 0.2f;
        private MouseButton _lastClick;

        public Vector2 MousePosition =>
            new Vector2(_lastMouseState.X, _lastMouseState.Y);

        public bool IsPrimaryMouseDown
            => (Settings.SwapPrimaryMouseButton)
                ? _lastMouseState.RightButton == ButtonState.Pressed
                : _lastMouseState.LeftButton == ButtonState.Pressed;
        
        /// <summary>
        /// Occurs when the mouse is moved.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        
        /// <summary>
        /// Occurs when a mouse button is pressed.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        
        /// <summary>
        /// Occurs when a mouse button is released.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        
        /// <summary>
        /// Occurs when the mouse wheel is scrolled vertically.
        /// </summary>
        public event EventHandler<MouseScrollEventArgs> MouseScroll;
        
        /// <summary>
        /// Occurs when the mouse wheel is scrolled horizontally.
        /// </summary>
        public event EventHandler<MouseScrollEventArgs> MouseHorizontalScroll;
        
        /// <summary>
        /// Occurs when text is typed into the game.
        /// </summary>
        public event EventHandler<KeyEventArgs> TextInput;
        
        /// <summary>
        /// Occurs when a key is pressed or repeated.
        /// </summary>
        public event EventHandler<KeyEventArgs> KeyDown;
        
        /// <summary>
        /// Occurs when a key is released.
        /// </summary>
        public event EventHandler<KeyEventArgs> KeyUp;
        
        /// <summary>
        /// Occurs when a mouse button is double-clicked.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseDoubleClick;

        private SettingsService Settings
            => GetModule<SettingsService>();
        
        private void BindKeyboardEvents()
        {
            GameUtils.Log("Binding to keyboard input events...");

            GameLoop.Window.KeyDown += GameKeyDown;
            GameLoop.Window.KeyUp += GameKeyUp;
            GameLoop.Window.TextInput += GameTextInput;
        }

        private Vector2 PointToLocal(Vector2 point)
        {
            var px = (point.X / GameLoop.Window.ClientBounds.Width);
            var py = (point.Y / GameLoop.Window.ClientBounds.Height);

            return new Vector2(px * GameLoop.LocalWidth, py * GameLoop.LocalHeight);
        }
        
        private MouseState GetMouseState(GameWindow win)
        {
            var state = Mouse.GetState(win);
            var localPoint = PointToLocal(new Vector2(state.X, state.Y));

            return new MouseState((int) localPoint.X, (int) localPoint.Y, state.ScrollWheelValue, state.LeftButton,
                state.MiddleButton, state.RightButton, state.XButton1, state.XButton2,
                state.HorizontalScrollWheelValue);
        }
        
        private void GameTextInput(object sender, TextInputEventArgs e)
        {
            var modifiers = GetModifiers();
            TextInput?.Invoke(this, new KeyEventArgs(e.Key, modifiers, e.Character));
        }

        private void GameKeyUp(object sender, InputKeyEventArgs e)
        {
            var modifiers = GetModifiers();
            var ev = new KeyEventArgs(e.Key, modifiers);
            
            // Handle system keys.
            switch (ev.Key)
            {
                // dev console
                case Keys.F1:
                    if (GameLoop.DevConsole.IsOpen)
                        GameLoop.DevConsole.Close();
                    else
                        GameLoop.DevConsole.Open();
                    break;
                // Take Screenshot.
                case Keys.F2:
                    GameLoop.TakeScreenshot();
                    break;
                #if DEBUG
                // Developer: Toggle GUI hover highlight
                case Keys.F4:
                    Settings.HighlightHoveredGuiElement = !Settings.HighlightHoveredGuiElement;
                    break;
                #endif
                // Full-screen toggle.
                case Keys.F11:
                    if (Settings.FullScreenMode == FullScreenMode.Windowed)
                    {
                        Settings.FullScreenMode = FullScreenMode.FullScreen;
                    }
                    else
                    {
                        Settings.FullScreenMode = FullScreenMode.Windowed;
                    }
                    break;
                // Dark theme toggle for the UI.
                case Keys.F12:
                    Settings.EnableDarkTheme = !Settings.EnableDarkTheme;
                    break;
            }
            
            KeyUp?.Invoke(this, ev);
        }

        private ModifierKeys GetModifiers()
        {
            var state = Keyboard.GetState();
            var m = ModifierKeys.None;

            if (state.IsKeyDown(Keys.LeftControl) || state.IsKeyDown(Keys.RightControl))
                m |= ModifierKeys.Control;
            if (state.IsKeyDown(Keys.LeftAlt) || state.IsKeyDown(Keys.RightAlt))
                m |= ModifierKeys.Alt;
            if (state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift))
                m |= ModifierKeys.Shift;
            
            return m;
        }
        
        private void GameKeyDown(object sender, InputKeyEventArgs e)
        {
            var modifiers = GetModifiers();
            KeyDown?.Invoke(this, new KeyEventArgs(e.Key, modifiers));
        }
        
        private void HandleButtonEvent(MouseState state, MouseButton button, ButtonState prevState, ButtonState currentState)
        {
            if (prevState != currentState)
            {
                if (_clickCooldown > 0 && currentState == ButtonState.Released)
                {
                    _clickCooldown = 0;
                    if (_lastClick == button)
                    {
                        var ev = new MouseButtonEventArgs(state, button, currentState);
                        MouseDoubleClick?.Invoke(this, ev);
                    }
                }
                else
                {
                    var ev = new MouseButtonEventArgs(state, button, currentState);

                    if (currentState == ButtonState.Pressed)
                    {
                        MouseDown?.Invoke(this, ev);
                    }
                    else
                    {
                        MouseUp?.Invoke(this, ev);
                        _lastClick = button;
                        _clickCooldown = _clickCooldownStart;
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void OnUpdate(GameTime gameTime)
        {
            _clickCooldown = Math.Max(0, _clickCooldown - (float) gameTime.ElapsedGameTime.TotalSeconds);
            var mouseState = GetMouseState(GameLoop.Window);

            // Handle button events.
            if (Settings.SwapPrimaryMouseButton)
            {
                // Swap right and left click internally so royce is happy lol.
                HandleButtonEvent(mouseState, MouseButton.Left, _lastMouseState.RightButton, mouseState.RightButton);
                HandleButtonEvent(mouseState, MouseButton.Right, _lastMouseState.LeftButton, mouseState.LeftButton);
            }
            else
            {
                HandleButtonEvent(mouseState, MouseButton.Left, _lastMouseState.LeftButton, mouseState.LeftButton);
                HandleButtonEvent(mouseState, MouseButton.Right, _lastMouseState.RightButton, mouseState.RightButton);
            }
            
            HandleButtonEvent(mouseState, MouseButton.Middle, _lastMouseState.MiddleButton,
                mouseState.MiddleButton);

            // And now let's do the browser buttons.
            HandleButtonEvent(mouseState, MouseButton.X1, _lastMouseState.XButton1, mouseState.XButton1);
            HandleButtonEvent(mouseState, MouseButton.X2, _lastMouseState.XButton2, mouseState.XButton2);

            // Vertical scrolling.
            if (mouseState.ScrollWheelValue != _lastMouseState.ScrollWheelValue)
            {
                MouseScroll?.Invoke(this, new MouseScrollEventArgs(mouseState, ScrollDirection.Vertical, mouseState.ScrollWheelValue, (mouseState.ScrollWheelValue - _lastMouseState.ScrollWheelValue)));
            }

            // Horizontal scrolling.
            if (mouseState.HorizontalScrollWheelValue != _lastMouseState.HorizontalScrollWheelValue)
            {
                MouseHorizontalScroll?.Invoke(this, new MouseScrollEventArgs(mouseState, ScrollDirection.Horizontal, mouseState.HorizontalScrollWheelValue, (mouseState.HorizontalScrollWheelValue - _lastMouseState.HorizontalScrollWheelValue)));
            }


            // Handle mouse movement.
            if (mouseState.X != _lastMouseState.X || mouseState.Y != _lastMouseState.Y)
            {
                var deltaX = mouseState.X - _lastMouseState.X;
                var deltaY = mouseState.Y - _lastMouseState.Y;

                MouseMove?.Invoke(this, new MouseMoveEventArgs(mouseState, deltaX, deltaY));
            }

            _lastMouseState = mouseState;
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            BindKeyboardEvents();
        }
    }
}
