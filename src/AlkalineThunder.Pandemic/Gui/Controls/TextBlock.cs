using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A simple control that displays a block of text.
    /// </summary>
    [MarkupElement("text")]
    public sealed class TextBlock : Control
    {
        private string _text = "";
        private TextWrappingMode _wrapMode = TextWrappingMode.WordWrap;
        private FontStyle _font = SkinFontStyle.Paragraph;
        private TextTransform _transform;
        private string _wrapped = "";
        
        /// <summary>
        /// Represents a way that text can be transformed when rendering in a <see cref="TextBlock"/>.
        /// </summary>
        public enum TextTransform
        {
            /// <summary>
            /// Nothing is done to the text.
            /// </summary>
            None,
            
            /// <summary>
            /// All characters in the text will be lowercase.
            /// </summary>
            Lowercase,
            
            /// <summary>
            /// All characters in the text will be uppercase.
            /// </summary>
            Uppercase
        }

        /// <summary>
        /// Gets or sets the transformation of text on the text block.
        /// </summary>
        [MarkupProperty("transform")]
        public TextTransform Transform
        {
            get => _transform;
            set
            {
                if (_transform != value)
                {
                    _transform = value;
                    InvalidateMeasure();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        [MarkupProperty("color")]
        public ControlColor TextColor { get; set; } = ControlColor.Text;

        /// <summary>
        /// Gets or sets the text to be displayed.
        /// </summary>
        [MarkupProperty("text")]
        public string Text
        {
            get => _text;
            set
            {
                value ??= "";
                
                if (_text != value)
                {
                    _text = value;
                    InvalidateMeasure();
                }
            }
        }

        private string TransformedText
        {
            get
            {
                switch (_transform)
                {
                    case TextTransform.None:
                        return _text;
                    case TextTransform.Uppercase:
                        return _text.ToUpper();
                    case TextTransform.Lowercase:
                        return _text.ToLower();
                    default:
                        return _text;
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the behaviour of the text block when wrapping the text.
        /// </summary>
        [MarkupProperty("wrap")]
        public TextWrappingMode WrapMode
        {
            get => _wrapMode;
            set
            {
                if (_wrapMode != value)
                {
                    _wrapMode = value;
                    InvalidateMeasure();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the horizontal alignment of the text.
        /// </summary>
        [MarkupProperty("align")]
        public TextAlign TextAlign { get; set; } = TextAlign.Left;

        /// <summary>
        /// Gets or sets the font used to draw the text.
        /// </summary>
        [MarkupProperty("font")]
        public FontStyle Font
        {
            get => _font;
            set
            {
                if (_font != value)
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));

                    _font = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var font = Font.GetFont(this);
            var text = TransformedText;

            if (alottedSize.X > 0)
                text = TextRenderer.WrapText(font, text, alottedSize.X - Margin.Horizontal, WrapMode);

            var measurement = font.MeasureString(text);

            _wrapped = text;

            return measurement;
        }
        
        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var color = TextColor.GetColor(this);
            var font = Font.GetFont(this);
            
            renderer.Begin();

            renderer.DrawString(font, _wrapped,  ContentRectangle.Location.ToVector2(), color);
            
            renderer.End();
        }
    }
}
