using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AlkalineThunder.Pandemic.Animation;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Scenes;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Provides the base functionality for all GUI elements.
    /// </summary>
    public abstract class Control : IGuiContext
    {
        private Scene _scene;

        SceneSystem IGuiContext.SceneSystem => this.SceneSystem;
        
        /// <summary>
        /// Contains an internal list of a control's children.
        /// </summary>
        protected sealed class ControlCollection : ICollection<Control>
        {
            private List<Control> _children = new List<Control>();
            private Control _owner;
            
            /// <summary>
            /// Creates a new instance of the <see cref="ControlCollection"/> class.
            /// </summary>
            /// <param name="owner">The <see cref="Control"/> to which this collection belongs.</param>
            public ControlCollection(Control owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            /// <summary>
            /// Gets the amount of children which belong to this collection's owning control.
            /// </summary>
            public int Count => _children.Count;

            /// <summary>
            /// Gets a value indicating whether the value is read-only.
            /// </summary>
            public bool IsReadOnly => false;

            /// <summary>
            /// Returns the child control at the specified index.
            /// </summary>
            /// <param name="index">The index of the control to find.</param>
            public Control this[int index] => _children[index];

            /// <summary>
            /// Adds a control to the collection.
            /// </summary>
            /// <param name="item">The control to add to the collection.</param>
            /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
            /// <exception cref="InvalidOperationException"><paramref name="item"/> already belongs to another parent control.</exception>
            public void Add(Control item)
            {
                if (item == null)
                    throw new ArgumentNullException(nameof(item));

                if (item.Parent != null)
                    throw new InvalidOperationException("Specified item already has a parent element.");

                item.Parent = _owner;
                _children.Add(item);
                
                _owner.InvalidateMeasure();
            }

            /// <summary>
            /// Removes all controls from the collection.
            /// </summary>
            public void Clear()
            {
                while (_children.Count > 0)
                {
                    Remove(_children[0]);
                }
            }

            /// <summary>
            /// Determines whether the given item is contained in the collection.
            /// </summary>
            /// <param name="item">The item to find.</param>
            /// <returns>A value indicating whether the collection contains the item.</returns>
            public bool Contains(Control item)
            {
                return item != null && item.Parent == _owner;
            }

            /// <summary>
            /// You tell me what this does.
            /// </summary>
            /// <param name="array">You tell me what this is for.</param>
            /// <param name="arrayIndex">You tell me what this is for.</param>
            public void CopyTo(Control[] array, int arrayIndex)
            {
                _children.CopyTo(array, arrayIndex);
            }

            /// <summary>
            /// Returns some stupid .NET thing that allows foreach loops to work
            /// </summary>
            /// <returns>...some stupid .NET thing that allows foreach loops to work</returns>
            public IEnumerator<Control> GetEnumerator()
            {
                return _children.GetEnumerator();
            }

            /// <summary>
            /// Removes the given item from the collection.
            /// </summary>
            /// <param name="item">The item to remove.</param>
            /// <returns>Whether the collection even contained the item in the first place.</returns>
            public bool Remove(Control item)
            {
                if (item == null)
                    return false;
                
                if (!Contains(item))
                    return false;

                item.Parent = null;
                _children.Remove(item);
                _owner.InvalidateMeasure();
                return true;
            }

            /// <summary>
            /// Returns some stupid .NET thing that allows foreach loops to work
            /// </summary>
            /// <returns>...some stupid .NET thing that allows foreach loops to work</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return _children.GetEnumerator();
            }
        }

        private Animator _animator;
        private bool _layoutVisible = true;
        private Rectangle _scissorRect;
        private bool _needsArrangement = true;
        private HorizontalAlignment _hAlign;
        private VerticalAlignment _vAlign;
        private bool _enabled = true;
        private bool _visible = true;
        private float _minWidth;
        private float _maxWidth;
        private float _fixedWidth;
        private float _minHeight;
        private float _maxHeight;
        private float _fixedHeight;
        private Padding _padding;
        private Padding _margin;
        private bool _measurementValid;
        private SceneSystem _topLevelSceneSystem;
        private List<IAttachedProperty> _props = new List<IAttachedProperty>();
        private float _opacity = 1;
        private Transform _transform = new Transform();

        public Transform Transform
        {
            get => _transform;
            set
            {
                if (_transform != value)
                {
                    _transform = value ?? throw new ArgumentNullException(nameof(value));
                }
            }
        }
        
        public float Opacity
        {
            get => _opacity;
            set
            {
                var clamped = MathHelper.Clamp(value, 0, 1);
                if (Math.Abs(clamped - _opacity) > 0.0001f)
                {
                    _opacity = clamped;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the control is visible to the layout system.
        /// </summary>
        public bool LayoutVisible
        {
            get => _layoutVisible;
            set
            {
                if (_layoutVisible != value)
                {
                    _layoutVisible = value;
                    InvalidateMeasure();
                }
            }
        }
        
        /// <summary>
        /// Gets a reference to the parent of this <see cref="Control"/>.  Will return null if the control has no parent.
        /// </summary>
        public Control Parent { get; private set; }
        
        /// <summary>
        /// Gets a value representing the layout bounds of the control.
        /// </summary>
        public Rectangle BoundingBox { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Scene"/> that this control is attached to.
        /// </summary>
        /// <exception cref="InvalidOperationException">The control has a <see cref="Parent"/> and thus cannot be attached to a scene.</exception>
        public Scene Scene
        {
            get => (Parent != null) ? Parent.Scene : _scene;
            set
            {
                if (Parent != null)
                    throw new InvalidOperationException("Cannot set owner scene of a non-toplevel control.");

                if (value == null)
                {
                    _scene = null;
                }
                else
                {
                    if (_scene != value)
                    {
                        if (_scene != null)
                            throw new InvalidOperationException("Cannot change the scene of a bound control.");
                        
                        _scene = value;
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled and can receive input
        /// events.
        /// </summary>
        [MarkupProperty("enabled")]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    InvalidateMeasure();
                }
            }
        }

        public SkinSystem Skin => SceneSystem.Skin;
        
        /// <summary>
        /// Gets or sets a value indicating whether the control is visible on-screen.
        /// </summary>
        [MarkupProperty("visible")]
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the horizontal alignment of the control within the bounds of its
        /// <see cref="Parent"/>.
        /// </summary>
        [MarkupProperty("horizontal-align")]
        public HorizontalAlignment HorizontalAlignment
        {
            get => _hAlign;
            set
            {
                if (_hAlign != value)
                {
                    _hAlign = value;
                    InvalidateMeasure();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating the vertical alignment of the control within the bounds of its
        /// <see cref="Parent"/>.
        /// </summary>
        [MarkupProperty("vertical-align")]
        public VerticalAlignment VerticalAlignment
        {
            get => _vAlign;
            set
            {
                if (_vAlign != value)
                {
                    _vAlign = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the fixed width of the control.
        /// </summary>
        [MarkupProperty("width")]
        public float FixedWidth
        {
            get => _fixedWidth;
            set
            {
                if (Math.Abs(_fixedWidth - value) > 0.01f)
                {
                    _fixedWidth = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the fixed height of the control.
        /// </summary>
        [MarkupProperty("height")]
        public float FixedHeight
        {
            get => _fixedHeight;
            set
            {
                if (Math.Abs(_fixedHeight - value) > .01f)
                {
                    _fixedHeight = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum desired width of the control.
        /// </summary>
        [MarkupProperty("min-width")]
        public float MinWidth
        {
            get => _minWidth;
            set
            {
                _minWidth = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the minimum desired height of the control.
        /// </summary>
        [MarkupProperty("min-height")]
        public float MinHeight
        {
            get => _minHeight;
            set
            {
                _minHeight = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the maximim desired width of the control.
        /// </summary>
        [MarkupProperty("max-width")]
        public float MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the maximum desired height of the control.
        /// </summary>
        [MarkupProperty("max-height")]
        public float MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = value;
                InvalidateMeasure();
            }
        }
        
        /// <summary>
        /// Gets a value representing the calculated desired size of the control.
        /// </summary>
        public Vector2 DesiredSize { get; private set; }

        public float ComputedOpacity
            => (Parent != null) ? (Opacity * Parent.ComputedOpacity) : Opacity;
        
        /// <summary>
        /// Gets a value indicating the location of the control on-screen.
        /// </summary>
        public Vector2 Location => BoundingBox.Location.ToVector2();

        /// <summary>
        /// Gets the location of the control's content rectangle.
        /// </summary>
        public Vector2 ContentLocation => ContentRectangle.Location.ToVector2();
        
        /// <summary>
        /// Gets or sets the inner margin of the control.
        /// </summary>
        [MarkupProperty("margin")]
        public Padding Margin
        {
            get => _margin;
            set
            {
                if (_margin != value)
                {
                    _margin = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the control or any of its children have keyboard focus.
        /// </summary>
        public bool HasAnyFocus => SceneSystem.IsFocused(this) || this.InternalChildren.Any(x => x.HasAnyFocus);

        /// <summary>
        /// Gets or sets the ID of the control.  The ID can be used to find this instance later on and is primarily used in the markup system.
        /// </summary>
        [MarkupProperty("id")] public string Id { get; set; } = "";
        
        /// <summary>
        /// Gets or sets an arbitrary data tag to assign to this control.
        /// </summary>
        public object Tag { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating the padding of the control in relation to its content.
        /// </summary>
        [MarkupProperty("padding")]
        public Padding Padding
        {
            get => _padding;
            set
            {
                if (_padding != value)
                {
                    _padding = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets a value representing the control's content rectangle.
        /// </summary>
        public Rectangle ContentRectangle { get; private set; } 
        
        /// <summary>
        /// Gets or sets the <see cref="SceneSystem"/> to which this control belongs.
        /// </summary>
        /// <exception cref="InvalidOperationException">The control has a <see cref="Parent"/> control or has already been claimed by another <see cref="SceneSystem"/>.</exception>
        public SceneSystem SceneSystem
        {
            get => _topLevelSceneSystem ?? (Parent != null ? Parent.SceneSystem : null);
            set
            {
                if (Parent != null)
                {
                    throw new InvalidOperationException("Cannot set the GuiManager of a child control.");
                }
                
                _topLevelSceneSystem = value;
            }
        }

        /// <summary>
        /// Invalidates the control's arrangement, forcing the layout system to recalculate it.
        /// </summary>
        public void InvalidateArrangement()
        {
            if (!_needsArrangement)
            {
                Parent?.InvalidateMeasure();

                _needsArrangement = true;

                foreach (var child in CollapseControlTree())
                    child.InvalidateArrangement();
            }
        }
        
        /// <summary>
        /// Finds a child control of the specified type with the specified <see cref="Id"/>.
        /// </summary>
        /// <param name="id">The desired ID to find.</param>
        /// <typeparam name="T">The specific type of control to look for.</typeparam>
        /// <returns>The instance of the found control or null if none were found.</returns>
        public T FindById<T>(string id) where T : Control
        {
            foreach (var child in InternalChildren)
            {
                var found = child.FindById<T>(id);
                if (found != null)
                    return found;
            }
            
            if (this is T typed && typed.Id == id)
                return typed;
            else
                return null;
        }

        public Animator Animator => _animator;
        
        /// <summary>
        /// Gets a <see cref="ControlCollection"/> containing the children of the control.
        /// </summary>
        protected ControlCollection InternalChildren { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="Control"/> class.
        /// </summary>
        public Control()
        {
            InternalChildren = new ControlCollection(this);
            
            _animator = new Animator();
        }

        /// <summary>
        /// Called during control layout calculation when the layout system needs to measure the size
        /// of the control's content.
        /// </summary>
        /// <param name="alottedSize">The size inside which the measured content must fit.</param>
        /// <returns>A value representing the desired width and height of the control's content.</returns>
        /// <remarks>
        /// By default, this method will always return a size of 0 unless overridden by a derived class.  When
        /// the layout system calls this method, it will pass the values of the control's <see cref="MaxWidth"/> and
        /// <see cref="MaxHeight"/> properties in <paramref name="alottedSize"/>.  If the aforementioned properties are set
        /// to 0, then so will the respective values of the <paramref name="alottedSize"/> vector.  In this case, it is
        /// generally assumed that the control is allowed to be infinitely large in a given dimension.
        ///
        /// The <paramref name="alottedSize"/> value passed by the layout system is only a soft recommendation to make
        /// certain measurement implementations easier.  As the return value of MeasureOverride is only a desired size, the
        /// layout system will take care of making sure that the control's content fits within the calculated layout bounds.
        /// </remarks>
        protected virtual Vector2 MeasureOverride(Vector2 alottedSize)
        {
            return Vector2.Zero;
        }

        /// <summary>
        /// Called by the layout system to give the control a chance to arrange its content into the calculated bounding
        /// rectangle.
        /// </summary>
        /// <param name="bounds">A rectangle representing the control's content bounds.</param>
        /// <remarks>
        /// Unless implemented by a derived class, this method will do absolutely nothing by default. It simply
        /// exists to provide container controls a chance to lay out their children.  The layout system will calculate
        /// the desired size of the control, apply any width and height constraints, followed by applying the control's
        /// <see cref="Padding"/> and then aligning the control within it's <see cref="Parent"/> using the given
        /// <see cref="HorizontalAlignment"/> and <see cref="VerticalAlignment"/> properties.  The resulting bounding rectangle
        /// will be passed to <see cref="Arrange"/> within the <paramref name="bounds"/> parameter, which itself should be
        /// equal to <see cref="ContentRectangle"/>.  All child controls must fit within these calculated bounds, or simply
        /// not be displayed on-screen.
        /// </remarks>
        protected virtual void Arrange(Rectangle bounds)
        {
            
        }
        
        /// <summary>
        /// Called once per render cycle when it is time for the control to paint itself on-screen.
        /// </summary>
        /// <param name="renderer">A <see cref="SpriteRocket2D"/> object that's been pre-configured to render the control.</param>
        protected virtual void OnPaint(SpriteRocket2D renderer) { }

        /// <summary>
        /// Finds a child control where the given coordinates are within the control's <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="x">The horizontal coordinate to look for.</param>
        /// <param name="y">The vertical coordinate to look for.</param>
        /// <returns>An instance of the <see cref="Control"/> that was found, or null.</returns>
        /// <remarks>
        /// This method is recursive.  It will start by iterating through the control's <see cref="InternalChildren"/> in
        /// reverse order and calling <see cref="FindControl"/> on each child until a non-null value is returned.  If this happens,
        /// then the value will be returned by the initial <see cref="FindControl"/> call.  If not, then the given
        /// <paramref name="x"/> and <paramref name="y"/> values will be checked against this control's <see cref="BoundingBox"/>.
        /// If the coordinates are within the bounding box, then this control will be returned.  Otherwise, the function
        /// will return null.
        ///
        /// Confusing as fuck, right? Yeah, well, that's the beauty of recursion.  I hate it too.
        /// </remarks>
        public Control FindControl(int x, int y)
        {
            if (this.Visible && this.Enabled)
            {
                if (x >= BoundingBox.Left && x <= BoundingBox.Right
                                          && y >= BoundingBox.Top && y <= BoundingBox.Bottom)
                {
                    foreach (var child in InternalChildren.Reverse())
                    {
                        var grandChild = child.FindControl(x, y);
                        if (grandChild != null)
                            return grandChild;
                    }

                    return this;
                }
            }

            return null;
        }

        internal IEnumerable<Control> CollapseControlTree()
        {
            foreach (var child in InternalChildren)
            {
                foreach (var c in child.CollapseControlTree())
                    yield return c;
            }
            
            yield return this;
        }
        
        /// <summary>
        /// Invalidates the control's desired size measurement, causing the game to recalculate
        /// the size of the control.  Will bubble up to the top-level control.
        /// </summary>
        public void InvalidateMeasure()
        {
            if (_measurementValid)
            {
                _measurementValid = false;
                InvalidateArrangement();
            }
            
            Parent?.InvalidateMeasure();
        }

        /// <summary>
        /// Called once per every engine tick to give the control a chance to update if needed.
        /// </summary>
        /// <param name="gameTime">An object representing the time since the last engine tick.</param>
        public void Update(GameTime gameTime)
        {
            _animator.Update(gameTime);
            OnUpdate(gameTime);

            for (var i = 0; i < InternalChildren.Count; i++)
            {
                InternalChildren[i].Update(gameTime);
            }
        }

        /// <summary>
        /// Called every frame by the Pandemic Framework to allow the control to update.
        /// </summary>
        /// <param name="gameTime">The amount of time since the last update.</param>
        protected virtual void OnUpdate(GameTime gameTime)
        {
            
        }
        
        /// <summary>
        /// Paints the control and all of its children on-screen.
        /// </summary>
        /// <param name="gameTime">An object representing the time since the last engine tick.</param>
        /// <param name="renderer">The renderer to which the control should be drawn.</param>
        public void Draw(GameTime gameTime, SpriteRocket2D renderer)
        {
            if (this.Visible && (!this.BoundingBox.IsEmpty))
            {
                if (SceneSystem.EnableClipping)
                {
                    if (_scissorRect.IsEmpty)
                        return;

                    renderer.ClippingRectangle = _scissorRect;
                }

                renderer.RenderOpacity = ComputedOpacity;
                renderer.Transform.Position = Transform.Position;
                renderer.Transform.Rotation = Transform.Rotation;
                renderer.Transform.Scale = Transform.Scale;
                
                this.OnPaint(renderer);

                renderer.RenderOpacity = 1;
                renderer.Transform.Position = Vector2.Zero;
                renderer.Transform.Rotation = 0;
                renderer.Transform.Scale = Vector2.One;

                renderer.ClippingRectangle = Rectangle.Empty;
                
                foreach (var child in InternalChildren)
                {
                    child.Draw(gameTime, renderer);
                }

                if (!Enabled)
                {
                    renderer.Begin();
                    renderer.FillRectangle(BoundingBox, Color.Black * 0.5f);
                    renderer.End();
                }

            }
        }

        private Rectangle GetScissorRect()
        {
            var p = this;
            var rect = BoundingBox;
            
            while (p != null)
            {
                rect = Rectangle.Intersect(rect, p.BoundingBox);
                p = p.Parent;
            }

            var location = SceneSystem.GameLoop.PointToScreen(rect.Location.ToVector2());
            var size = SceneSystem.GameLoop.PointToScreen(rect.Size.ToVector2()) + Vector2.One;

            return new Rectangle((int) location.X, (int) location.Y, (int) size.X, (int) size.Y);
        }

        private Vector2 AccountForParentSizes(Vector2 alottedSize)
        {
            var p = Parent;
            while (p != null)
            {
                if (p.MaxWidth > 0 || p.FixedWidth > 0)
                {
                    var pw = (p.FixedWidth > 0) ? p.FixedWidth : p.MaxWidth;
                    
                    if (alottedSize.X <= 0 || pw <= alottedSize.X)
                        alottedSize.X = pw;
                }
                
                if (p.MaxHeight > 0 || p.FixedHeight > 0)
                {
                    var ph = (p.FixedHeight > 0) ? p.FixedHeight : p.MaxHeight;
                    
                    if (alottedSize.Y <= 0 || ph <= alottedSize.Y)
                        alottedSize.Y = ph;
                }
                
                p = p.Parent;
            }
            
            return alottedSize;
        }
        
        /// <summary>
        /// Calculates the desired size of the control and returns the result.
        /// </summary>
        /// <returns>The control's computed desired size.</returns>
        public Vector2 Measure(Vector2? reservedAreaSize = null, Vector2? alottment = null, bool ignoreValidation = false)
        {
            if (!_layoutVisible)
                return Vector2.Zero;
            
            if (!_measurementValid || ignoreValidation)
            {
                var alottedSize = alottment.GetValueOrDefault();
                var additional = reservedAreaSize.GetValueOrDefault();
                
                if (MaxWidth > 0)
                    alottedSize.X = MaxWidth;

                if (FixedWidth > 0)
                    alottedSize.X = FixedWidth;
                
                if (MaxHeight > 0)
                    alottedSize.Y = MaxHeight;

                if (FixedHeight > 0)
                    alottedSize.Y = FixedHeight;

                alottedSize = AccountForParentSizes(alottedSize);

                if (alottedSize.X > 0)
                    alottedSize.X = Math.Max(alottedSize.X - additional.X, 0);
                if (alottedSize.Y > 0)
                    alottedSize.Y = Math.Max(alottedSize.Y - additional.Y, 0);
                
                var measurement = MeasureOverride(alottedSize);

                if (measurement.X < MinWidth)
                    measurement.X = MinWidth;

                if (measurement.Y < MinHeight)
                    measurement.Y = MinHeight;

                if (measurement.X > MaxWidth && MaxWidth > 0)
                    measurement.X = MaxWidth;

                if (measurement.Y > MaxHeight && MaxHeight > 0)
                    measurement.Y = MaxHeight;

                if (FixedWidth > 0)
                    measurement.X = FixedWidth;

                if (FixedHeight > 0)
                    measurement.Y = FixedHeight;

                DesiredSize = measurement + Margin + Padding;
                
                _measurementValid = true;
            }

            return DesiredSize;
        }

        /// <summary>
        /// Performs a full layout calculation of the control within the given bounding box.
        /// </summary>
        /// <param name="bounds">A rectangle representing the bounds inside which the control should fit.</param>
        /// <remarks>
        /// This method can be computationally intensive as it performs a full layout calculation of the control and, in
        /// some cases, that of its children.  If possible, avoid calling this method unless an important value that would affect
        /// the layout of a control has changed.
        ///
        /// First, the control's content size will be measured.  This operation may be recursive in some cases as some types
        /// of controls may need to measure the size of their children in order to return an accurate measurement.
        ///
        /// The measurement returned will then be used to calculate a bounding box for the control.  This bounding box will be aligned
        /// within the specified <paramref name="bounds"/> using the desired size of the control and its <see cref="HorizontalAlignment"/>
        /// and <see cref="VerticalAlignment"/> properties.
        ///
        /// After this, the control's <see cref="Padding"/> will be applied to calculate the control's <see cref="ContentRectangle"/>
        /// after which the control will be given a chance to lay out its children.
        /// </remarks>
        public void Layout(Rectangle bounds)
        {
            if (!_layoutVisible)
            {
                DesiredSize = Vector2.Zero;
                BoundingBox = Rectangle.Empty;
                ContentRectangle = BoundingBox;
                _scissorRect = Rectangle.Empty;
                return;
            }
            
            if (_needsArrangement)
            {
                if (bounds == Rectangle.Empty)
                {
                    this.BoundingBox = Rectangle.Empty;
                    this._scissorRect = Rectangle.Empty;
                }
                else
                {
                    // Calculate the size of the control's content.
                    var actualSize = this.Measure();

                    // Based on this, calculate the control's bounding box aligned to the rectangle passed to this method.
                    // Top-level controls will align to the screen rectangle.
                    // Child controls will align to the rectangle given to them by their parent.
                    this.BoundingBox =
                        LayoutUtils.CalculateBoundingBox(bounds, HorizontalAlignment, VerticalAlignment, actualSize);
                    this.BoundingBox = this.Padding.Deflate(BoundingBox);
                    
                    this._scissorRect = GetScissorRect();
                    this.ContentRectangle = this.Margin.Deflate(this.BoundingBox);
                    
                    // This allows the control to arrange itself and its children inside the calculated bounding box.
                    // This behaviour is COMPLETELY up to the implementation of the control.
                    this.Arrange(this.ContentRectangle);
                }

                _needsArrangement = false;
            }
        }

        /// <summary>
        /// Determines whether the given <paramref name="control"/> is an ancestor of this control.
        /// </summary>
        /// <param name="control">An instance of the possible ancestor control.</param>
        /// <returns>A value indicating whether <paramref name="control"/> is an ancestor of this control.</returns>
        public bool HasParent(Control control)
        {
            var p = this;
            while (p != null)
            {
                if (p == control)
                    return true;
                p = p.Parent;
            }

            return false;
        }
        
        internal bool InvokeMouseMove(MouseMoveEventArgs e)
            => OnMouseMove(e);
        
        internal bool InvokeMouseEnter(MouseMoveEventArgs e)
            => OnMouseEnter(e);

        internal bool InvokeMouseLeave(MouseMoveEventArgs e)
            => OnMouseLeave(e);

        internal bool InvokeMouseDown(MouseButtonEventArgs e)
            => OnMouseDown(e);

        internal bool InvokeMouseUp(MouseButtonEventArgs e)
            => OnMouseUp(e);

        internal bool InvokeGainedFocus(FocusEventArgs e)
            => OnGainedFocus(e);

        internal bool InvokeLostFocus(FocusEventArgs e)
            => OnLostFocus(e);

        internal bool InvokeClick(MouseButtonEventArgs e)
            => OnClick(e);

        internal bool InvokeDoubleClick(MouseButtonEventArgs e)
            => OnDoubleClick(e);

        
        internal bool InvokeTextInput(KeyEventArgs e)
            => OnTextInput(e);

        internal bool InvokeKeyDown(KeyEventArgs e)
                => OnKeyDown(e);

        internal bool InvokeKeyUp(KeyEventArgs e)
            => OnKeyUp(e);

        internal bool InvokeMouseScroll(MouseScrollEventArgs e)
            => OnMouseScroll(e);
        
        /// <inheritdoc cref="InvokeMouseMove" />
        protected virtual bool OnMouseMove(MouseMoveEventArgs e)
        {
            if (MouseMove != null)
            {
                MouseMove(this, e);
                return true;
            }

            return false;
        }

        /// <inheritdoc cref="InvokeMouseEnter" />
        protected virtual bool OnMouseEnter(MouseMoveEventArgs e)
        {
            if (MouseEnter != null)
            {
                MouseEnter(this, e);
                return true;
            }

            return false;
        }

        /// <inheritdoc cref="InvokeMouseLeave" />        
        protected virtual bool OnMouseLeave(MouseMoveEventArgs e)
        {
            if (MouseLeave != null)
            {
                MouseLeave(this, e);
                return true;
            }

            return false;
        }

        /// <inheritdoc cref="InvokeMouseDown" />
        protected virtual bool OnMouseDown(MouseButtonEventArgs e)
        {
            if (MouseDown != null)
            {
                MouseDown(this, e);
                return true;
            }

            return false;
        }

        /// <inheritdoc cref="InvokeMouseUp" />
        protected virtual bool OnMouseUp(MouseButtonEventArgs e)
        {
            if (MouseUp != null)
            {
                MouseUp(this, e);
                return true;
            }

            return false;
        }

        /// <inheritdoc cref="InvokeGainedFocus" />
        protected virtual bool OnGainedFocus(FocusEventArgs e)
        {
            if (GainedFocus != null)
            {
                GainedFocus(this, e);
                return true;
            }

            return false;
        }
        
        /// <inheritdoc cref="InvokeLostFocus" />
        protected virtual bool OnLostFocus(FocusEventArgs e)
        {
            if (LostFocus != null)
            {
                LostFocus(this, e);
                return true;
            }

            return false;
        }

        /// <inheritdoc cref="InvokeClick" />
        protected virtual bool OnClick(MouseButtonEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Occurs when the control is double-clicked.
        /// </summary>
        /// <param name="e">The arguments of the mouse event.</param>
        /// <returns>Whether or not the event was handled.</returns>
        protected virtual bool OnDoubleClick(MouseButtonEventArgs e)
        {
            if (DoubleClick != null)
            {
                DoubleClick(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Occurs when the user types a character into the control.
        /// </summary>
        /// <param name="e">The arguments of the keyboard event.</param>
        /// <returns>Whether or not the event was handled.</returns>
        protected virtual bool OnTextInput(KeyEventArgs e)
        {
            if (TextInput != null)
            {
                TextInput?.Invoke(this, e);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Occurs wheb a key is pressed.
        /// </summary>
        /// <param name="e">The arguments of the keyboard event.</param>
        /// <returns>Whether or not the keyboard event was handled.</returns>
        protected virtual bool OnKeyDown(KeyEventArgs e)
        {
            if (KeyDown != null)
            {
                KeyDown?.Invoke(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Occurs when the mouse wheel is scrolled.
        /// </summary>
        /// <param name="e">The arguments of the scroll wheel event.</param>
        /// <returns>Whether or not the event was handled.</returns>
        protected virtual bool OnMouseScroll(MouseScrollEventArgs e)
        {
            if (MouseScroll != null)
            {
                MouseScroll(this, e);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Occurs when a keyboard key is released.
        /// </summary>
        /// <param name="e">The arguments of the keyboard event.</param>
        /// <returns>Whether or not the keyboard event was handled.</returns>
        protected virtual bool OnKeyUp(KeyEventArgs e)
        {
            if (KeyUp != null)
            {
                KeyUp?.Invoke(this, e);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Occurs when the mouse moves within the bounds of the control.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> MouseMove;
        
        /// <summary>
        /// Occurs when the mouse enters the bounds of the control.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> MouseEnter;
        
        /// <summary>
        /// Occurs when the mouse leaves the bounds of the control.
        /// </summary>
        public event EventHandler<MouseMoveEventArgs> MouseLeave;
        
        /// <summary>
        /// Occurs when a mouse button is pressed while the mouse is hovering over the control.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        
        /// <summary>
        /// Occurs when a mouse button is released while the mouse is hovering over the control.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        
        /// <summary>
        /// Occurs when the control gains keyboard focus.
        /// </summary>
        public event EventHandler<FocusEventArgs> GainedFocus;
        
        /// <summary>
        /// Occurs when the control loses keyboard focus.
        /// </summary>
        public event EventHandler<FocusEventArgs> LostFocus;
        
        /// <summary>
        /// Occurs when a mouse button is clicked while the mouse is hovering over the control.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> Click;

        /// <summary>
        /// Occurs when a mouse button is double-clicked.
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> DoubleClick;

        /// <summary>
        /// Occurs when the mouse wheel is scrolled.
        /// </summary>
        public event EventHandler<MouseScrollEventArgs> MouseScroll;
        
        /// <summary>
        /// Occurs when a character is typed.
        /// </summary>
        public event EventHandler<KeyEventArgs> TextInput;
        
        /// <summary>
        /// Occurs when a key is pressed.
        /// </summary>
        public event EventHandler<KeyEventArgs> KeyDown;
        
        /// <summary>
        /// Occurs when a key is released.
        /// </summary>
        public event EventHandler<KeyEventArgs> KeyUp;

        
        /// <summary>
        /// Determines whether the object has an attached property with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the attached property to look for.</param>
        /// <param name="type">The type of value stored in the attached property.</param>
        /// <returns>A value indicating whether an attached property with the given name and type was found.</returns>
        /// <seealso cref="HasAttachedProperty{T}"/>
        public bool HasAttachedProperty(string name, Type type)
        {
            return _props.Any(x => x.Name == name && x.Value.GetType() == type);
        }

        /// <summary>
        /// Determines whether the object has an attached property with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the attached property to look for.</param>
        /// <typeparam name="T">The type of value stored in the attached property.</typeparam>
        /// <returns>A value indicating whether an attached property was found matching the given name and type.</returns>
        /// <seealso cref="HasAttachedProperty"/>
        public bool HasAttachedProperty<T>(string name)
        {
            return _props.Any(x => x.Name == name && x.Value is T);
        }

        /// <summary>
        /// Retrieves the value of an attached property.
        /// </summary>
        /// <param name="name">The name of the desired property.</param>
        /// <returns>The value of the attached property, or null if the property does not exist.</returns>
        public object GetAttachedProperty(string name)
        {
            var prop = _props.FirstOrDefault(x => x.Name == name);

            return prop?.Value;
        }

        /// <summary>
        /// Retrieves the value of an attached property.
        /// </summary>
        /// <param name="name">The name of the attached property to retrieve.</param>
        /// <typeparam name="T">The type of the value stored in the property.</typeparam>
        /// <returns>The value of the attached property, or the default value of <typeparamref name="T"/> if the property was not found.</returns>
        public T GetAttachedProperty<T>(string name)
        {
            var prop = _props.FirstOrDefault(x => x.Name == name);

            if (prop is AttachedProperty<T>)
            {
                return (prop as AttachedProperty<T>).Value;
            }
            else if (prop != null)
            {
                return (T) prop.Value;
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Sets the value of an attached property.
        /// </summary>
        /// <param name="name">The name of the attached property to set.</param>
        /// <param name="value">The new value of the attached property.</param>
        /// <typeparam name="T">The type of the value to be stored in the property.</typeparam>
        public void SetAttachedProperty<T>(string name, T value)
        {
            var existingProp = _props.FirstOrDefault(x => x.Name == name);

            if (existingProp != null)
            {
                if (value == null)
                {
                    _props.Remove(existingProp);
                    InvalidateMeasure();
                    return;
                }

                existingProp.Value = value;
                InvalidateMeasure();
            }
            else
            {
                var prop = new AttachedProperty<T>();
                prop.Name = name;
                prop.Value = value;
                _props.Add(prop);
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Sets an attached property on this object.
        /// </summary>
        /// <param name="name">The name of the attached property.</param>
        /// <param name="value">The value to store in the property.</param>
        public void SetAttachedProperty(string name, object value)
        {
            var prop = _props.FirstOrDefault(x => x.Name == name);
            if (prop != null)
            {
                _props.Remove(prop);
            }

            if (value != null)
            {
                var newProp = new NonGenericAttachedProperty(name, value);
                _props.Add(newProp);
            }
        }
    }
}
