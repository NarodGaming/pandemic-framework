using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A control that represents a single choice in a multiple-choice question.
    /// </summary>
    [MarkupElement("radio")]
    public class RadioButton : ContentControl
    {
        private Texture2D _unmarkedTexture;
        private Texture2D _markedTexture;
        private const int Spacing = 4;
        private const int RadioSize = 16;
        private bool _marked;

        /// <summary>
        /// Gets or sets a value indicating whether the radio button is marked or filled in.
        /// </summary>
        [MarkupProperty("value")]
        public bool IsMarked
        {
            get => _marked;
            set
            {
                if (_marked != value)
                {
                    _marked = value;
                    OnMarkedChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the radio's bubble.
        /// </summary>
        [MarkupProperty("color")]
        public ControlColor MarkColor { get; set; } = ControlColor.Text;

        /// <summary>
        /// Occurs when the radio's bubble is filled in.
        /// </summary>
        public event EventHandler MarkedChanged;

        /// <summary>
        /// Creates a new instance of the <see cref="RadioButton"/> control.
        /// </summary>
        public RadioButton()
        {
            _unmarkedTexture = GameLoop.LoadTexture("Icons/radiobox-blank");
            _markedTexture = GameLoop.LoadTexture("Icons/radiobox-marked");
        }
        
        /// <summary>
        /// Called when the radio's bubble is filled in.
        /// </summary>
        protected virtual void OnMarkedChanged()
        {
            MarkedChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var alottedWithoutCheck = Math.Max(0, alottedSize.X - (RadioSize + Spacing));
            var m = base.MeasureOverride(new Vector2(alottedWithoutCheck, alottedSize.Y));

            return new Vector2(m.X, Math.Max(m.Y, RadioSize));
        }

        /// <inheritdoc />
        protected override bool OnMouseUp(MouseButtonEventArgs e)
        {
            IsMarked = true;
            return base.OnMouseUp(e);
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            bounds.X += RadioSize + Spacing;
            bounds.Width -= (RadioSize + Spacing);
            base.Arrange(bounds);
        }

        private Texture2D GetMarkTexture()
        {
            return _marked ? _unmarkedTexture : _markedTexture;
        }
        
        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var fg = MarkColor.GetColor(this);
            var markBounds = new Rectangle(ContentRectangle.Left,
                ContentRectangle.Top + ((ContentRectangle.Height - RadioSize) / 2), RadioSize, RadioSize);

            renderer.Begin();
            renderer.FillRectangle(markBounds, fg, GetMarkTexture());
            renderer.End();
        }
    }
}