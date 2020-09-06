using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    [MarkupElement("markdown")]
    internal class MarkdownDisplay : Control
    {
        private string _text = string.Empty;
        private StackPanel _stack = new StackPanel();


        [MarkupProperty("text")]
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value ?? string.Empty;
                    ProcessMarkdown();
                }
            }
        }

        public MarkdownDisplay()
        {
            InternalChildren.Add(_stack);
        }
        
        private void ProcessMarkdown()
        {
            _stack.Clear();
        }
        
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            return _stack.Measure(null, alottedSize);
        }

        protected override void Arrange(Rectangle bounds)
        {
            _stack.Layout(bounds);
        }
    }
}