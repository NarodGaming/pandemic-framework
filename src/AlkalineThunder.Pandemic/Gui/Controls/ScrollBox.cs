using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A control that allows it's content to be vertically scrolled.
    /// </summary>
    [MarkupElement("scroller")]
    public sealed class ScrollBox : Control
    {
        private const int ScrollBarWidth = 12;
        private Box _contentBox;
        private int _scrollOffset;
        
        /// <summary>
        /// Creates a new instance of the <see cref="ScrollBox"/> control.
        /// </summary>
        public ScrollBox()
        {
            _contentBox = new Box();
            _contentBox.BackgroundColor = "#00000000";
            
            
            InternalChildren.Add(_contentBox);
        }

        /// <summary>
        /// Gets or sets the content of the scroll box.
        /// </summary>
        public Control Content
        {
            get => _contentBox.Content;
            set => _contentBox.Content = value;
        }

        /// <summary>
        /// Scrolls to the top of the content.
        /// </summary>
        public void ScrollToTop()
        {
            _scrollOffset = 0;
            InvalidateArrangement();
        }

        /// <summary>
        /// Scrolls to the bottom of the content.
        /// </summary>
        public void ScrollToBottom()
        {
            if (_contentBox.BoundingBox.Height > BoundingBox.Height)
            {
                _scrollOffset = _contentBox.BoundingBox.Height - BoundingBox.Height;
                InvalidateArrangement();
            }
            else
            {
                _scrollOffset = 0;
                InvalidateArrangement();
            }
        }

        /// <summary>
        /// Scrolls to the specified position in the scroll box.
        /// </summary>
        /// <param name="offset">The position to be scrolled to.</param>
        public void ScrollTo(int offset)
        {
            var scrollMax = (_contentBox.BoundingBox.Height - ContentRectangle.Height);

            scrollMax = Math.Max(scrollMax, 0);
            offset = Math.Max(offset, 0);

            offset = Math.Min(offset, scrollMax);

            _scrollOffset = offset;
            InvalidateArrangement();
        }

        /// <inheritdoc />
        protected override bool OnMouseScroll(MouseScrollEventArgs e)
        {
            ScrollTo(_scrollOffset - (e.WheelDelta / 4));
            return true;
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var maxWidth = Math.Max(0, alottedSize.X - ScrollBarWidth);
            var contentMeasure = _contentBox.Measure(null, new Vector2(maxWidth, 0));
            
            contentMeasure.X += ScrollBarWidth;

            return new Vector2(contentMeasure.X, contentMeasure.Y);
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            bounds.Width -= ScrollBarWidth;

            var desiredSize = _contentBox.Measure();

            var contentRect = new Rectangle(
                bounds.Left,
                bounds.Top - _scrollOffset,
                bounds.Width,
                (int) desiredSize.Y
            );

            _contentBox.Layout(contentRect);
        }
    }
}