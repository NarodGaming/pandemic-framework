using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A simple drop-down item chooser.
    /// </summary>
    [MarkupElement("combo")]
    public class ComboBox : ItemsControl<string>
    {
        private Vector2 _lastLocation;
        private Button _button;
        private TextBlock _currentDisplay;
        private SelectList _dropdown;
        private Box _dropdownBox;
        
        /// <summary>
        /// Creates a new instance of the <see cref="ComboBox"/> control.
        /// </summary>
        public ComboBox()
        {
            _currentDisplay = new TextBlock();
            _dropdown = new SelectList();
            var dropdownArrow = new Icon
            {
                FixedWidth = 16,
                FixedHeight = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Image = GameLoop.LoadTexture("Icons/chevron-down")
            };
            _button = new Button();
            
            var sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 4
            };

            sp.AddChild(_currentDisplay);
            sp.AddChild(dropdownArrow);

            _button.Content = sp;

            InternalChildren.Add(_button);

            _button.Click += HandleButtonClick;
            _dropdown.SelectedIndexChanged += HandleItemChanged;

            _currentDisplay.Text = "Select...";

            _dropdownBox = new Box
            {
                MaxHeight = 75,
                MinHeight = 4,
                Content = new ScrollBox
                {
                    Content = _dropdown
                }
            };

            _button.LostFocus += HandleLostFocus;
        }

        private void HandleLostFocus(object sender, FocusEventArgs e)
        {
            if (_dropdownBox.SceneSystem != null)
                _dropdownBox.Scene.Gui.RemoveChild(_dropdownBox);
        }
        
        /// <inheritdoc />
        protected override void OnItemAdded(string item)
        {
            _dropdown.Add(item);
            base.OnItemAdded(item);
        }

        /// <inheritdoc />
        protected override void OnItemRemoved(string item)
        {
            _dropdown.Remove(item);
            base.OnItemRemoved(item);
        }

        /// <inheritdoc />
        protected override void OnSelectedItemChanged(int index)
        {
            if (_dropdown.SelectedIndex != index)
                _dropdown.SelectedIndex = index;

            _currentDisplay.Text = (SelectedIndex != -1) ? SelectedItem : "Select...";
            base.OnSelectedItemChanged(index);
        }

        /// <inheritdoc />
        protected override void OnUpdate(GameTime gameTime)
        {
            if (_lastLocation != Location)
            {
                Scene.Gui.RemoveChild(_dropdownBox);
                _lastLocation = Location;
            }
            
            base.OnUpdate(gameTime);
        }

        private void HandleButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (_dropdownBox.Parent == null)
            {
                Scene.Gui.AddChild(_dropdownBox);
                SetDropDownLayout();
            }
        }

        private void SetDropDownLayout()
        {
            SceneSystem.SetFocus(_dropdown);
            
            // Dropdown won't cover the entire screen now.
            _dropdownBox.SetAttachedProperty(CanvasPanel.AnchorProperty, CanvasPanel.Anchor.TopLeft);
            _dropdownBox.SetAttachedProperty(CanvasPanel.AutoSizeProperty, true);
            
            // Force the dropdown to our width
            _dropdownBox.FixedWidth = ContentRectangle.Width;
            
            // Measure the box.
            var m = _dropdownBox.Measure();
            
            // Figure out where, vertically, the box should be placed.
            var y = Math.Min(ContentRectangle.Bottom, SceneSystem.BoundingBox.Height - m.Y);
            
            // Set the position on the gui canvas.
            _dropdownBox.SetAttachedProperty(CanvasPanel.PositionProperty, new Vector2(ContentRectangle.Left, y));
        }

        private void HandleItemChanged(object sender, EventArgs e)
        {
            if (_dropdown.SelectedIndex != SelectedIndex)
                SelectedIndex = _dropdown.SelectedIndex;

            Scene?.Gui.RemoveChild(_dropdownBox);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            return _button.Measure(null, alottedSize);
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            _button.Layout(bounds);
        }
    }
}