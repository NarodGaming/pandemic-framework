using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A simple single-line text input.
    /// </summary>
    [MarkupElement("input")]
    public sealed class TextInput : Control
    {
        private const int LineHeight = 2;
        private const int LineMargin = 2;

        private int _oldCursorPos;
        private float _textDrawOffset;
        private string _text = "";
        private int _cursorIndex;
        private FontStyle _font = SkinFontStyle.Input;
        private string _hint = "";
        private int _maxChars;
        private float _textHeight;
        
        /// <summary>
        /// Gets or sets the text color.
        /// </summary>
        [MarkupProperty("color")]
        public ControlColor TextColor { get; set; } = ControlColor.Text;
        
        /// <summary>
        /// Gets or sets the color of the text underline when the control has focus.
        /// </summary>
        [MarkupProperty("active")]
        public ControlColor ActiveLineColor { get; set; } = ControlColor.Primary;

        /// <summary>
        /// Gets or sets the hint text.
        /// </summary>
        [MarkupProperty("label")]
        public string HintText
        {
            get => _hint;
            set
            {
                if (_hint != value)
                {
                    _hint = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum amount of characters allowed in the text input.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The specified value is negative.</exception>
        [MarkupProperty("length")]
        public int MaxCharacters
        {
            get => _maxChars;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (value != _maxChars)
                {
                    _maxChars = value;
                    if (_text.Length > _maxChars)
                    {
                        _text = _text.Substring(0, _maxChars);
                        if (_cursorIndex > _text.Length)
                            _cursorIndex = _text.Length;
                        InvalidateMeasure();
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the text of the input.
        /// </summary>
        [MarkupProperty("text")]
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    if (_cursorIndex > _text.Length)
                        _cursorIndex = _text.Length;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font used to render the text.
        /// </summary>
        /// <exception cref="ArgumentNullException">The given font is invalid.</exception>
        [MarkupProperty("font")]
        public FontStyle Font
        {
            get => _font;
            set
            {
                if (value != _font)
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));

                    _font = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <inheritdoc />
        protected override bool OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Left:
                    _cursorIndex = Math.Max(_cursorIndex - 1, 0);
                    InvalidateMeasure();
                    break;
                case Keys.Right:
                    _cursorIndex = Math.Min(_text.Length, _cursorIndex + 1);
                    InvalidateMeasure();
                    break;
                case Keys.Home:
                    _cursorIndex = 0;
                    InvalidateMeasure();
                    break;
                case Keys.End:
                    _cursorIndex = _text.Length;
                    InvalidateMeasure();
                    break;
            }

            return base.OnKeyDown(e);
        }

        /// <inheritdoc />
        protected override bool OnTextInput(KeyEventArgs e)
        {
            switch (e.Character)
            {
                case '\b':
                    if (_cursorIndex > 0)
                    {
                        _cursorIndex--;
                        _text = _text.Remove(_cursorIndex, 1);
                        InvalidateMeasure();
                        return true;
                    }

                    break;
                default:
                    if (MaxCharacters > 0 && _text.Length >= MaxCharacters)
                        return base.OnTextInput(e);

                    _text = _text.Insert(_cursorIndex, e.Character.ToString());
                    _cursorIndex++;
                    InvalidateMeasure();
                    return true;
            }

            return base.OnTextInput(e);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var font = Font.GetFont(this);
            
            _textHeight = font.MeasureString("#").Y;
            
            var textMeasure = font.MeasureString(Text.StripNewLines());
            
            return new Vector2(textMeasure.X, Math.Max(_textHeight, textMeasure.Y) + LineMargin + LineHeight);
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            if (_oldCursorPos != _cursorIndex)
            {
                var font = Font.GetFont(this);
                var textMeasure = font.MeasureString(_text.StripNewLines());

                if (textMeasure.X <= bounds.Width)
                {
                    _textDrawOffset = 0;
                }
                else
                {
                    if (_cursorIndex > _oldCursorPos)
                    {
                        var delta = _cursorIndex - _oldCursorPos;
                        var deltaText = _text.Substring(_oldCursorPos, delta);
                        var deltaX = font.MeasureString(deltaText).X;

                        var oldTextWidth = font.MeasureString(_text.Substring(0, _oldCursorPos)).X;

                        var oldVisualPosition = (bounds.Left - _textDrawOffset) + oldTextWidth;

                        if (deltaX > bounds.Width) deltaX -= bounds.Width;
                        
                        if (oldVisualPosition + deltaX >= bounds.Right)
                        {
                            _textDrawOffset += deltaX;
                        }
                    }
                    else if (_cursorIndex < _oldCursorPos)
                    {
                        var cursorToText = _text.Substring(0, _cursorIndex);
                        var textLength = font.MeasureString(cursorToText).X;
                        var visualStart = (bounds.Left - _textDrawOffset);
                        var visualCursorPos = visualStart + textLength;

                        if (textLength <= bounds.Width)
                        {
                            _textDrawOffset = 0;
                        }
                        else
                        {
                            while (visualCursorPos <= (bounds.Right - (bounds.Width / 2)))
                            {
                                var prevCharIndex = _cursorIndex - 1;

                                if (prevCharIndex < 0)
                                {
                                    _textDrawOffset = 0;
                                    break;
                                }

                                var charMeasure = font.MeasureString(_text[prevCharIndex].ToString()).X;

                                _textDrawOffset -= charMeasure;

                                visualCursorPos += charMeasure;
                            }
                        }
                    }
                }
                
                _oldCursorPos = _cursorIndex;
            }
            
            base.Arrange(bounds);
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var font = Font.GetFont(this);
            var lineColor = (HasAnyFocus ? this.ActiveLineColor : this.TextColor).GetColor(this);
            var textColor = TextColor.GetColor(this);
            var bounds = ContentRectangle;

            renderer.Begin();

            if (string.IsNullOrEmpty(_text))
            {
                renderer.DrawString(font, _hint, new Vector2(bounds.Left, bounds.Top), textColor * 0.5f);

                if (HasAnyFocus)
                {
                    renderer.FillRectangle(new Rectangle(bounds.Left, bounds.Top, 2, (int) _textHeight), textColor);
                }
            }
            else
            {
                renderer.DrawString(font, _text, new Vector2(bounds.Left - _textDrawOffset, bounds.Top), textColor);

                if (HasAnyFocus)
                {
                    var textToCursor = _text.Substring(0, _cursorIndex);
                    var textMeasure = font.MeasureString(textToCursor);
                    var cursorX = (bounds.Left + (int) textMeasure.X) - _textDrawOffset;
                    
                    renderer.FillRectangle(new Rectangle((int)cursorX, bounds.Top, 2, (int) _textHeight), textColor);
                }
            }

            renderer.FillRectangle(new Rectangle(bounds.Left, bounds.Bottom - LineHeight, bounds.Width, LineHeight),
                lineColor);
            
            renderer.End();
        }
    }
}