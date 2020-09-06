using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Skinning;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A clickable user interface element that can contain a single aligned child.
    /// </summary>
    [MarkupElement("button")]
    public sealed class Button : ContentControl
    {
        private ControlColor _textColor = SkinColor.ButtonText;
        private bool _hovered;
        private bool _pressed;

        /// <summary>
        /// Gets or sets a value indicating whether the button should draw it's background
        /// when the button isn't pressed or being hovered.  Use this if you want the button to be
        /// transparent.
        /// </summary>
        [MarkupProperty("draw-idle-bg")]
        public bool DrawIdleBackground { get; set; } = true;

        /// <summary>
        /// Gets or sets the background color of the button.
        /// </summary>
        [MarkupProperty("bg")]
        public ControlColor ButtonColor { get; set; } = SkinColor.Button;

        /// <summary>
        /// Gets or sets the color of button text.
        /// </summary>
        public ControlColor TextColor
        {
            get => _textColor;
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    if (Content is TextBlock text)
                    {
                        text.TextColor = _textColor;
                    }
                }
            }
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="Button"/> class.
        /// </summary>
        public Button()
        {
            Margin = new Padding(7, 4);
            ContentHorizontalAlignment = HorizontalAlignment.Center;
            ContentVerticalAlignment = VerticalAlignment.Stretch;
        }

        /// <inheritdoc />
        protected override bool OnMouseEnter(MouseMoveEventArgs e)
        {
            _hovered = true;
            return base.OnMouseEnter(e);
        }

        /// <inheritdoc />
        protected override bool OnMouseLeave(MouseMoveEventArgs e)
        {
            _hovered = false;
            return base.OnMouseLeave(e);
        }

        /// <inheritdoc />
        protected override bool OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                _pressed = true;
            }
            
            return base.OnMouseDown(e);
        }

        /// <inheritdoc />
        protected override bool OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                _pressed = false;
            }
            
            return base.OnMouseDown(e);
        }

        /// <inheritdoc />
        protected override void OnContentChanged(Control content)
        {
            if (content is TextBlock text)
            {
                text.TextColor = _textColor;
            }
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var color = ButtonColor.GetColor(this);
            var drawBg = DrawIdleBackground;

            if (_pressed)
            {
                color = color.Darken(0.15f);
                drawBg = true;
            }
            else if (_hovered)
            {
                color = color.Lighten(0.15f);
                drawBg = true;
            }

            if (drawBg)
            {
                renderer.Begin();
                renderer.FillRectangle(BoundingBox, color);
                renderer.End();
            }
        }
    }
}