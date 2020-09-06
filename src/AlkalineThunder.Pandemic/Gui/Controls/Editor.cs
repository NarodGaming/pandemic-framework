using System;
using System.Linq;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SpriteFontPlus;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A word-wrapping multi-line text editor control that supports line numbers.
    /// </summary>
    [MarkupElement("editor")]
    public class Editor : Control
    {
        private int _xOffset;
        private int _yOffset;
        private int _lnSpacing = 2;
        private int _lnGutter = 4;
        private int _cursorX;
        private int _cursorY;
        private string[] _lines = new string[] {""};
        private bool _showLineNumbers = true;
        private FontStyle _font = SkinFontStyle.Code;
        private float _lineNumbersWidth;
        private int _lineHeight;
        private string[] _renderLines;
        private int _renderCursorX;
        private int _renderCursorY;
        
        /// <summary>
        /// Gets or sets whether line numbeers are enabled.
        /// </summary>
        [MarkupProperty("line-numbers")]
        public bool LineNumbers
        {
            get => _showLineNumbers;
            set
            {
                if (_showLineNumbers != value)
                {
                    _showLineNumbers = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font used by text and line numbers.
        /// </summary>
        [MarkupProperty("font")]
        public FontStyle Font
        {
            get => _font;
            set
            {
                if (_font != value)
                {
                    _font = value ?? throw new ArgumentNullException(nameof(value));
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color of the editor.
        /// </summary>
        [MarkupProperty("bg")]
        public ControlColor BackgroundColor { get; set; } = ControlColor.Default;
        
        /// <summary>
        /// Gets or sets the text color of the line numbers.
        /// </summary>
        [MarkupProperty("line-numbers-color")]
        public ControlColor LineNumberColor { get; set; } = SkinColor.EditorGutterText;
        
        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        [MarkupProperty("color")]
        public ControlColor TextColor { get; set; } = SkinColor.EditorText;
        
        /// <summary>
        /// Gets or sets the highlight color of the active line.
        /// </summary>
        [MarkupProperty("active-bg")]
        public ControlColor ActiveLineColor { get; set; } = SkinColor.EditorHighlight;
        
        /// <summary>
        /// Gets or sets the background of the line numbers area.
        /// </summary>
        [MarkupProperty("line-numbers-bg")]
        public ControlColor LineNumbersBackground { get; set; } = SkinColor.EditorGutter;
        
        /// <summary>
        /// Gets or sets the text of the editor.
        /// </summary>
        public string Text
        {
            get => string.Join(Environment.NewLine, _lines);
            set
            {
                if (Text != value)
                {
                    _lines = value.Replace("\r", "").Split('\n');
                    _cursorX = 0;
                    _cursorY = 0;
                    InvalidateMeasure();
                }
            }
        }

        private void HandleBackspace()
        {
            if (_cursorX > 0)
            {
                _cursorX--;
                _lines[_cursorY] = _lines[_cursorY].Remove(_cursorX, 1);
                InvalidateMeasure();
            }
            else
            {
                if (_cursorY > 0)
                {
                    var lineAtCursor = _lines[_cursorY];
                    _cursorY--;
                    _cursorX = _lines[_cursorY].Length;
                    _lines[_cursorY] += lineAtCursor;
                    for (var i = _cursorY + 1; i < _lines.Length - 1; i++)
                    {
                        _lines[i] = _lines[i + 1] ?? "";
                    }

                    Array.Resize(ref _lines, _lines.Length - 1);
                    
                    InvalidateMeasure();
                }
            }
        }

        private void HandleDelete()
        {
            if (_cursorX < (_lines[_cursorY] ?? "").Length)
            {
                _lines[_cursorY] = _lines[_cursorY].Remove(_cursorX, 1);
                InvalidateMeasure();
            }
            else
            {
                if (_cursorY < _lines.Length - 1)
                {
                    _lines[_cursorY] += _lines[_cursorY + 1];
                    
                    for (var i = _cursorY + 1; i < _lines.Length - 1; i++)
                    {
                        _lines[i] = _lines[i + 1] ?? "";
                    }

                    Array.Resize(ref _lines, _lines.Length - 1);
                    
                    InvalidateMeasure();
                }
            }
        }

        private void HandleCursorRight()
        {
            var line = _lines[_cursorY] ?? "";

            if (_cursorX < line.Length)
            {
                _cursorX++;
                InvalidateMeasure();
            }
            else
            {
                if (_cursorY < _lines.Length - 1)
                {
                    _cursorY++;
                    _cursorX = 0;
                    InvalidateMeasure();
                }
            }
        }
        
        private void HandleCursorLeft()
        {
            if (_cursorX > 0)
            {
                _cursorX--;
                InvalidateMeasure();
            }
            else
            {
                if (_cursorY > 0)
                {
                    _cursorY--;
                    _cursorX = _lines[_cursorY].Length;
                    InvalidateMeasure();
                }
            }
        }

        private void HandleCursorDown()
        {
            if (_cursorY < _lines.Length - 1)
            {
                _cursorY++;
                if (_cursorX > _lines[_cursorY].Length)
                    _cursorX = _lines[_cursorY].Length;
                InvalidateMeasure();
            }
        }
        
        private void HandleCursorUp()
        {
            if (_cursorY > 0)
            {
                _cursorY--;
                if (_cursorX > _lines[_cursorY].Length)
                    _cursorX = _lines[_cursorY].Length;
                InvalidateMeasure();
            }
        }

        private void HandleEnter()
        {
            var lineAfterCursor = (_lines[_cursorY] ?? "").Substring(_cursorX);
            _lines[_cursorY] = (_lines[_cursorY] ?? "").Substring(0, _cursorX);
            
            Array.Resize(ref _lines, _lines.Length + 1);
            _cursorY++;
            _cursorX = 0;
                    
            for (var i = _lines.Length - 1; i > _cursorY; i--)
            {
                _lines[i] = _lines[i - 1];
            }

            _lines[_cursorY] = lineAfterCursor;
                    
            InvalidateMeasure();

        }

        /// <inheritdoc />
        protected override bool OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.Delete:
                    HandleDelete();
                    break;
                case Keys.Home:
                    _cursorX = 0;
                    InvalidateMeasure();
                    break;
                case Keys.End:
                    _cursorX = (_lines[_cursorY] ?? "").Length;
                    InvalidateMeasure();
                    break;
                case Keys.Left:
                    HandleCursorLeft();
                    break;
                case Keys.Right:
                    HandleCursorRight();
                    break;
                case Keys.Up:
                    HandleCursorUp();
                    break;
                case Keys.Down:
                    HandleCursorDown();
                    break;
            }
            
            return base.OnKeyDown(e);
        }

        /// <inheritdoc />
        protected override bool OnTextInput(KeyEventArgs e)
        {
            if (e.Key == Keys.Delete)
                return base.OnTextInput(e);
            
            switch (e.Character)
            {
                case '\b':
                    HandleBackspace();
                    break;
                case '\r':
                    HandleEnter();
                    break;
                default:
                    var ln = _lines[_cursorY] ?? "";
                    ln = ln.Insert(_cursorX, e.Character.ToString());
                    _lines[_cursorY] = ln;
                    _cursorX++;
                    InvalidateMeasure();
                    break;
            }

            return base.OnTextInput(e);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var font = Font.GetFont(this);
            var text = string.Join(Environment.NewLine, _lines);
            var textMeasure = font.MeasureString(text);

            _lineHeight = (int) font.MeasureString("#").Y;
            
            if (_showLineNumbers)
            {
                var lastLineNumber = (_lines.Length - 1).ToString();
                var lineMeasure = font.MeasureString(lastLineNumber);
                textMeasure.X += lineMeasure.X + _lnGutter + _lnSpacing;
                _lineNumbersWidth = lineMeasure.X + _lnGutter;
            }
            else
            {
                _lineNumbersWidth = 0;
            }

            textMeasure.Y = Math.Max(textMeasure.Y, font.LineSpacing);

            return textMeasure;
        }

        private void DoRenderWrapping(DynamicSpriteFont font, Rectangle bounds)
        {
            var wrapWidth = bounds.Width - (_lineNumbersWidth);
            _renderLines = new string[_lines.Length];
            var ri = 0;

            for (var i = 0; i < _lines.Length; i++)
            {
                var line = _lines[i];

                if (i == _cursorY)
                {
                    var toCursor = line.Substring(0, _cursorX);
                    
                    var tcWrapped = TextRenderer.WrapText(font, toCursor, bounds.Width, TextWrappingMode.WordWrap);
                    var tcLines = tcWrapped.Split('\n');
                    _renderCursorY = ri + (tcLines.Length - 1);
                    _renderCursorX = tcLines.Last().Length;

                    var alcatel = tcLines.Length - 1;
                    
                    tcWrapped = TextRenderer.WrapText(font, line, bounds.Width, TextWrappingMode.WordWrap);
                    tcLines = tcWrapped.Split('\n');

                    if (tcLines[alcatel].Length < _renderCursorX)
                    {
                        var diff = _renderCursorX - tcLines[alcatel].Length;
                        _renderCursorX = diff;
                        _renderCursorY++;
                    }
                    
                }
                
                var wrapped = TextRenderer.WrapText(font, line, wrapWidth, TextWrappingMode.WordWrap);

                var wrapLines = wrapped.Split('\n');
                
                if (ri + wrapLines.Length >= _renderLines.Length)
                    Array.Resize(ref _renderLines, ri + wrapLines.Length);
                
                for (var j = 0; j < wrapLines.Length; j++)
                {
                    if (ri == _renderCursorY)
                    {
                        var l = wrapLines[0].Length;
                        if (_renderCursorX > l)
                        {
                            _renderCursorY++;
                            _renderCursorX -= l;
                        }
                    }
                    _renderLines[ri] = wrapLines[j];
                    ri++;
                }
            }
        }
        
        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            var font = Font.GetFont(this);

            DoRenderWrapping(font, bounds);
            
            var cursorYOnScreen = (bounds.Top + (_lineHeight * _renderCursorY)) - _yOffset;

            if (cursorYOnScreen > (bounds.Bottom - _lineHeight))
            {
                _yOffset += (cursorYOnScreen - (bounds.Bottom - _lineHeight));
            }
            else if (cursorYOnScreen < bounds.Top)
            {
                var diff = (bounds.Top - cursorYOnScreen);
                _yOffset = Math.Max(0, _yOffset - diff);
            }

            var lineToCursor = _renderLines[_renderCursorY].Substring(0, _renderCursorX);
            var cursorXOnScreen = (bounds.Left + (int) (_lineNumbersWidth + (LineNumbers ? _lnSpacing : 0)) +
                                   (int) font.MeasureString(lineToCursor).X) - _xOffset;
            var lnArea = bounds.Left + (int) _lineNumbersWidth + (LineNumbers ? _lnSpacing : 0);
            
            
            if (cursorXOnScreen > bounds.Right)
            {
                _xOffset += cursorXOnScreen - bounds.Right;
            }
            else if (cursorXOnScreen < bounds.Left + lnArea)
            {
                var diff = (bounds.Left + lnArea) - cursorXOnScreen;
                _xOffset = Math.Max(0, _xOffset - diff);
            }
            
            base.Arrange(bounds);
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var font = Font.GetFont(this);
            var fg = TextColor.GetColor(this);
            var bg = BackgroundColor.GetColor(this);
            var lnColor = LineNumberColor.GetColor(this);
            var active = ActiveLineColor.GetColor(this);
            var bounds = ContentRectangle;
            var y = bounds.Top - _yOffset;
            var lnBg = LineNumbersBackground.GetColor(this);
            
            renderer.Begin();
            renderer.FillRectangle(bounds, bg);

            if (LineNumbers)
            {
                renderer.FillRectangle(new Rectangle(bounds.Left, bounds.Top, (int) _lineNumbersWidth, bounds.Height),
                    lnBg);
            }
            
            for (int i = 0; i < _renderLines.Length; i++)
            {
                var line = _renderLines[i] ?? "";
                var drawCursor = (i == _renderCursorY) && HasAnyFocus;

                if (drawCursor)
                {
                    renderer.FillRectangle(new Rectangle(bounds.Left, y, bounds.Width, _lineHeight), active);
                }
                
                var x = (bounds.Left + (int) _lineNumbersWidth + (LineNumbers ? _lnSpacing : 0)) - _xOffset;

                renderer.DrawString(font, line, new Vector2(x, y), fg);

                if (drawCursor)
                {
                    var toCursor = line.Substring(0, _renderCursorX);
                    var m2 = (int) font.MeasureString(toCursor).X;
                    renderer.FillRectangle(new Rectangle(x + m2, y, 2, _lineHeight), fg);
                }

                if (LineNumbers)
                {
                    renderer.FillRectangle(new Rectangle(bounds.Left, y, (int) _lineNumbersWidth, _lineHeight),
                        lnBg);
                    renderer.DrawString(font, i.ToString(), new Vector2(bounds.Left, y), lnColor);
                }

                y += _lineHeight;
            }
            
            renderer.End();

        }
    }
}