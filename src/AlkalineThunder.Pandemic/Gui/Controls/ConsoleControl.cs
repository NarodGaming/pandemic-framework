using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlkalineThunder.Pandemic.CommandLine;
using AlkalineThunder.Pandemic.CommandLine.Pty;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A user interface element that acts as a command-line console.
    /// </summary>
    [MarkupElement("console")]
    public sealed class ConsoleControl : Control, IDisposable
    {
        private Stack<string> _history = new Stack<string>();
        private Stack<string> _future = new Stack<string>();
        private int _cursorX;
        private int _cursorY;
        private string[] _lines;
        private string _text = "";
        private string _input = "";
        private PseudoTerminal _slave;
        private PseudoTerminal _master;
        private int _inputPos;
        private FontStyle _font = SkinFontStyle.Code;
        private TextWrappingMode _wrapMode = TextWrappingMode.WordWrap;
        private int _scrollOffsetInLines;
        private int _lineHeight;
        private float _baseFontSize;
        private float _zoomFactor = 1;
        
        /// <summary>
        /// Gets the input stream of the emulated pseudo-terminal.
        /// </summary>
        public StreamReader Input { get; private set; }
        
        /// <summary>
        /// Gets the output stream of the emulated pseudo-terminal.
        /// </summary>
        public StreamWriter Output { get; private set; }

        /// <summary>
        /// Gets or sets the console tab completion provider.
        /// </summary>
        public ITabCompletionSource TabCompletionSource { get; set; }
        
        /// <summary>
        /// Gets or sets whether the console emulates UNIX-style console interrupts (i.e, CTRL+C to halt a thread that's waiting for terminal input)
        /// </summary>
        public bool DoShellInterrupts { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the font used by the console.
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
        /// Gets or sets the background color of the console.
        /// </summary>
        [MarkupProperty("bg")]
        public ControlColor BackgroundColor { get; set; } = ControlColor.Default;

        /// <summary>
        /// Gets or sets the color of the console text.
        /// </summary>
        [MarkupProperty("color")]
        public ControlColor TextColor { get; set; } = ControlColor.Text;

        /// <summary>
        /// Gets or sets a value determining how text in the console should be wrapped.
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
        /// Makes the terminal and all of it's resources go bye-bye and your RAM usage not go high-high.  Sorry. I'm tired. I
        /// don't feel like monotonously regurgitating basic .NET framework concepts such as <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose()
        {
            _master?.Close();
            _slave?.Close();

            Input?.Close();
            Output?.Close();
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="ConsoleControl"/>.
        /// </summary>
        public ConsoleControl()
        {
            PseudoTerminal.CreatePair(out _master, out _slave);

            Input = new StreamReader(_master);
            Output = new StreamWriter(_master);

            Output.AutoFlush = true;
        }

        /// <summary>
        /// Writes the given text to the console.
        /// </summary>
        /// <param name="text">The text to output.</param>
        public void Write(string text)
            => Output.Write(text);

        /// <summary>
        /// Writes a line of text to the console.
        /// </summary>
        /// <param name="text">The text to write to the console.</param>
        public void WriteLine(string text)
            => Output.WriteLine(text);

        private void HandleHistory()
        {
            if (_history.Count > 0)
            {
                var i = _history.Pop();
                _input = i;
                _inputPos = i.Length;
                _future.Push(i);
                InvalidateMeasure();
            }
        }

        private void HandleFuture()
        {
            if (_future.Count > 0)
            {
                var i = _future.Pop();
                _input = i;
                _inputPos = i.Length;
                _history.Push(i);
                InvalidateMeasure();
            }
        }
        
        /// <inheritdoc />
        protected override bool OnKeyDown(KeyEventArgs e)
        {
            _scrollOffsetInLines = 0;
            
            switch (e.Key)
            {
                case Keys.OemMinus:
                    if (e.Control)
                    {
                        _zoomFactor = MathHelper.Clamp(_zoomFactor - 0.25f, 1, 4);
                        InvalidateMeasure();
                    }
                    break;
                case Keys.OemPlus:
                    if (e.Control)
                    {
                        _zoomFactor = MathHelper.Clamp(_zoomFactor + 0.25f, 1, 4);
                        InvalidateMeasure();
                    }
                    break;
                case Keys.Enter:
                    WriteLine(_input);

                    foreach (var c in  _input)
                    {
                        _slave.WriteByte((byte) c);
                    }

                    _slave.WriteByte((byte) '\r');
                    _slave.WriteByte((byte) '\n');

                    // submit the input to history
                    if (_history.Count == 0 || _history.Peek() != _input)
                    {
                        _history.Push(_input);
                    }

                    _future.Clear();
                    
                    _inputPos = 0;
                    _input = "";
                    InvalidateMeasure();
                    break;
                case Keys.Up:
                    HandleHistory();
                    break;
                case Keys.Down:
                    HandleFuture(); // That's a fucking metaphor for something.
                    break;
                case Keys.Left:
                    if (_inputPos > 0)
                    {
                        _inputPos--;
                        InvalidateMeasure();
                    }

                    break;
                case Keys.Right:
                    if (_inputPos < _input.Length)
                    {
                        _inputPos++;
                        InvalidateMeasure();
                    }

                    break;
                case Keys.Home:
                    _inputPos = 0;
                    InvalidateMeasure();
                    break;
                case Keys.End:
                    _inputPos = _input.Length;
                    InvalidateMeasure();
                    break;
                case Keys.Delete:
                    if (_inputPos < _input.Length)
                    {
                        _input = _input.Remove(_inputPos, 1);
                        InvalidateMeasure();
                    }
                    break;
                case Keys.C:
                    if (e.Control && DoShellInterrupts)
                    {
                        _slave.WriteByte((byte) KernelCharacters.ProcessInterruptSignal);
                    }
                    break;
                case Keys.Tab:
                    HandleTabCompletion();
                    break;
                default:
                    return base.OnKeyDown(e);
            }

            return base.OnKeyDown(e);
        }

        /// <inheritdoc />
        protected override bool OnTextInput(KeyEventArgs e)
        {
            _scrollOffsetInLines = 0;

            switch (e.Character)
            {
                case '\r':
                case '\n':
                case '\t':
                    break;
                case '\b':
                    if (_inputPos > 0)
                    {
                        _inputPos--;
                        _input = _input.Remove(_inputPos, 1);
                        InvalidateMeasure();
                    }

                    break;
                default:
                    _input = _input.Insert(_inputPos, e.Character.ToString());
                    _inputPos++;
                    InvalidateMeasure();
                    break;
            }

            return base.OnTextInput(e);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var font = Font.GetFont(this);

            _baseFontSize = font.Size;

            font.Size = (int)(_baseFontSize * _zoomFactor);
            
            var wrapped = TextRenderer.WrapText(font, _text + _input, alottedSize.X, WrapMode);
            _lineHeight = (int) font.MeasureString("#").Y;
            var m = font.MeasureString(wrapped);

            font.Size = (int) _baseFontSize;
            
            return m;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            // Get the font of the ternubK.
            var font = Font.GetFont(this);

            font.Size = (int) (_baseFontSize * _zoomFactor);
            
            // Get the input text from after the cursor.
            var inputAfterCursor = _input.Substring(_inputPos);
            
            // Get everything before the cursor.
            var everythingBeforeCursor = _text + _input.Substring(0, _inputPos);
            
            // Wrap everything before the cursor.
            var wrapPass1 = TextRenderer.WrapText(font, everythingBeforeCursor, bounds.Width, WrapMode);
            
            // Split that into lines.
            var lines = wrapPass1.Split('\n');
            
            // Cursor render position is easy to get from here.
            _cursorY = lines.Length - 1;
            _cursorX = lines[_cursorY].Length;

            // And now, we wrap the last line from above plus the input text after the cursor
            var wrapPass2 = TextRenderer.WrapText(font, lines[_cursorY] + inputAfterCursor, bounds.Width, WrapMode);
            
            // And we split THAT into lines.
            var additionalLines = wrapPass2.Split('\n');
            
            // Modify the last line in the first wrap
            lines[_cursorY] = additionalLines[0];
            
            // Resize the array to fit all the new lines
            Array.Resize(ref lines, lines.Length + (additionalLines.Length - 1));
            
            // Copy the lines over
            for (var i = 1; i < additionalLines.Length; i++)
            {
                lines[_cursorY + i] = additionalLines[i];
            }
            
            // Sometimes, the cursor's line may be wrapped during the second pass, such
            // that our _cursorX is now invalid (the line has less characters.) If this happens,
            // then we'll shift the cursor to the left by the length of its current line, and then
            // shift the cursor down one line.
            if (lines[_cursorY].Length < _cursorX)
            {
                _cursorX -= lines[_cursorY].Length;
                _cursorY++;
            }
            
            // Keep that for later:
            _lines = lines;

            font.Size = (int) _baseFontSize;
        }

        /// <inheritdoc />
        protected override bool OnMouseScroll(MouseScrollEventArgs e)
        {
            var amount = e.WheelDelta > 0 ? -2 : 2;
            _scrollOffsetInLines = Math.Max(_scrollOffsetInLines - amount, 0);
            return true;
        }

        /// <inheritdoc />
        protected override void OnUpdate(GameTime gameTime)
        {
            var b = 0;
            while ((b = _slave.ReadByte()) != -1)
            {
                _text += (char) b;

                _scrollOffsetInLines = 0;
                InvalidateMeasure();
            }
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var font = Font.GetFont(this);

            font.Size = (int) (_baseFontSize * _zoomFactor);
            
            var bgColor = BackgroundColor.GetColor(this);
            var fgColor = TextColor.GetColor(this);
            var bounds = ContentRectangle;
            var y = bounds.Bottom;
            
            renderer.Begin();

            renderer.FillRectangle(bounds, bgColor);
            
            for (var i = (_lines.Length - 1) - _scrollOffsetInLines; i >= 0; i--)
            {
                var line = _lines[i];
                var measure = font.MeasureString(line);
                y -= (int) Math.Max(_lineHeight, measure.Y);
                renderer.DrawString(font, line, new Vector2(bounds.Left, y), fgColor);

                if (i == _cursorY && HasAnyFocus)
                {
                    var toCursor = line.Substring(0, Math.Min(_cursorX, line.Length));
                    var toCursorMeasure = (int) font.MeasureString(toCursor).X;

                    renderer.FillRectangle(new Rectangle(bounds.Left + toCursorMeasure, y, 2, _lineHeight),
                        fgColor);
                }

                if (y < bounds.Top)
                    break;
            }
            
            renderer.End();

            font.Size = (int) _baseFontSize;
        }

        private void HandleTabCompletion()
        {
            if (string.IsNullOrWhiteSpace(_input))
                return;

            if (_inputPos < _input.Length)
            {
                _inputPos = _input.Length;
                return;
            }
            
            if (ShellUtils.TryTokenize(_input, out var tokens))
            {
                var lastTokenStart = _input.Length - tokens.Last().Length;
                var lastToken = tokens.Last();

                if (string.IsNullOrWhiteSpace(lastToken))
                    return;

                if (TabCompletionSource != null)
                {
                    var completions =
                        TabCompletionSource.AvailableCompletions.Where(x =>
                            x.ToLower().StartsWith(lastToken.ToLower())).OrderBy(x => x.Length);

                    if (completions.Any())
                    {
                        if (completions.Count() == 1)
                        {
                            _input = _input.Substring(0, lastTokenStart) + completions.First() + " ";
                            _inputPos = _input.Length;
                            InvalidateMeasure();
                        }
                        else
                        {
                            // grab the last line of output text
                            var lastLine = _text.Contains('\n') ? _text.Substring(_text.LastIndexOf('\n') + 1) : _text;
                            
                            // output a new line
                            WriteLine("");
                            
                            // get first 15 autocompletes
                            var sweetFifteen = completions.Take(15);
                            
                            // print them
                            var fifteen = sweetFifteen as string[] ?? sweetFifteen.ToArray();
                            foreach (var c in fifteen)
                            {
                                WriteLine(c);
                            }
                            
                            // output " -- more -- " if there are more
                            if (completions.Count() > fifteen.Length)
                                WriteLine(" -- more -- ");
                            
                            // repeat that last line
                            Write(lastLine);
                        }
                    }
                }
            }
        }
    }
}