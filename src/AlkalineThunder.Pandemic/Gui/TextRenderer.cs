using System;
using System.Linq;
using System.Text;
using SpriteFontPlus;

namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Provides extended text layout and rendering functionality for the engine.
    /// </summary>
    public static class TextRenderer
    {
        /// <summary>
        /// Wraps the given text so that it doesn't exceed the specified width.
        /// </summary>
        /// <param name="font">The font to use to measure the text.</param>
        /// <param name="text">The text to wrap the text.</param>
        /// <param name="maxLineWidth">The maximum width the text is not allowed to exceed.</param>
        /// <param name="mode">The wrapping algorithm to use.</param>
        /// <returns>The wrapped text.</returns>
        /// <exception cref="ArgumentNullException">The given font is invalid.</exception>
        /// <exception cref="ArgumentOutOfRangeException">We're not sure why.  We're not sure how.  But this is thrown if you somehow manage to pick a wrapping algorithm that just....flat out doesn't exist!</exception>
        public static string WrapText(DynamicSpriteFont font, string text, float maxLineWidth, TextWrappingMode mode)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (maxLineWidth <= 0)
                return text;

            if (mode == TextWrappingMode.None)
                return text;
            
            if (mode == TextWrappingMode.LetterWrap)
            {
                return LetterWrapInternal(font, text, maxLineWidth);
            }
            else if (mode == TextWrappingMode.WordWrap)
            {
                var sb = new StringBuilder();
                foreach (var line in text.Split('\n'))
                {
                    if (sb.Length > 0)
                        sb.Append('\n');
                    sb.Append(WordWrapInternal(font, line, maxLineWidth));
                }
                return sb.ToString();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
        
        private static string LetterWrapInternal(DynamicSpriteFont font, string text, float maxLineWidth)
        {
            var sb = new StringBuilder();
            var lineWidth = 0f;

            foreach (var c in text)
            {
                if (c == '\r') continue;
                if (c == '\n')
                {
                    sb.Append(c);
                    lineWidth = 0;
                    continue;
                }

                var m = font.MeasureString(c.ToString());
                if (lineWidth + m.X > maxLineWidth)
                {
                    sb.Append('\n');
                    lineWidth = 0;
                }

                lineWidth += m.X;
                sb.Append(c);
            }
            
            return sb.ToString();
        }
        
        private static string WordWrapInternal(DynamicSpriteFont font, string text, float lineWidth)
        {
            var sb = new StringBuilder();
            var word = new StringBuilder();
            var line = 0f;
            
            unsafe
            {
                var index = 0;
                var len = text.Length;

                fixed (char* chars = text)
                {
                    while (index < len)
                    {
                        var i = 0;
                        
                        for (i = index; i < len; i++)
                        {
                            var c = *(chars + i);
                            word.Append(c);
                            if (char.IsWhiteSpace(c))
                                break;
                        }

                        index = i + 1;

                        var wordMeasure = font.MeasureString(word.ToString());

                        if (wordMeasure.X > lineWidth)
                        {
                            if (line > 0)
                            {
                                sb.Append('\n');
                                line = 0;
                            }
                            
                            var letterWrapped = LetterWrapInternal(font, word.ToString(), lineWidth);
                            sb.Append(letterWrapped);
                            var lastLine = letterWrapped.Split('\n').Last();
                            line = font.MeasureString(lastLine).X;
                            word.Clear();
                            continue;
                        }

                        if (line + wordMeasure.X > lineWidth)
                        {
                            line = 0;
                            sb.Append('\n');
                        }
                        
                        sb.Append(word); 
                        line += wordMeasure.X;

                        word.Clear();
                    }
                }
            }

            return sb.ToString();
        }
    }
}
