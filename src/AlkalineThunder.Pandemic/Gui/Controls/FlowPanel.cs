using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A user interface element that will wrap its content.
    /// </summary>
    [MarkupElement("flowPanel")]
    public sealed class FlowPanel : ContainerControl
    {
        /// <summary>
        /// Represents a direction in which the content of a <see cref="FlowPanel"/> should be laid out.
        /// </summary>
        public enum FlowDirection
        {
            /// <summary>
            /// Items should be arranged from the left and wrap at the right edge.
            /// </summary>
            LeftToRight,
            
            /// <summary>
            /// Items should be arranged from the top, vertically, and wrap at the bottom edge.
            /// </summary>
            TopDown,
        }

        private FlowDirection _direction;
        private float _hSpacing;
        private float _vSpacing;

        /// <summary>
        /// Gets or sets a value indicating the way in which this container's content should be arranged.
        /// </summary>
        [MarkupProperty("direction")]
        public FlowDirection Direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    _direction = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal spacing between each child.
        /// </summary>
        [MarkupProperty("x-spacing")]
        public float HorizontalSpacing
        {
            get => _hSpacing;
            set
            {
                _hSpacing = value;
                InvalidateMeasure();
            }
        }

        /// <summary>
        /// Gets or sets the vertical spacing between each child.
        /// </summary>
        [MarkupProperty("y-spacing")]
        public float VerticalSpacing
        {
            get => _vSpacing;
            set
            {
                _vSpacing = value;
                InvalidateMeasure();
            }
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var size = Vector2.Zero;
            var lineWidth = 0f;
            var lineHeight = 0f;

            if (Direction == FlowDirection.LeftToRight)
            {
                foreach (var child in InternalChildren)
                {
                    var measure = child.Measure(null,new Vector2(alottedSize.X, 0));

                    if (lineWidth + measure.X + HorizontalSpacing > alottedSize.X && alottedSize.X > 0)
                    {
                        size.X = Math.Max(size.X, lineWidth);
                        lineWidth = measure.X + HorizontalSpacing;
                        size.Y += lineHeight + VerticalSpacing;
                        lineHeight = measure.Y;
                    }
                    else
                    {
                        lineWidth += measure.X + HorizontalSpacing;
                        lineHeight = Math.Max(lineHeight, measure.Y);
                    }
                }

                size.X = Math.Max(size.X, lineWidth);
                size.Y += lineHeight;
            }
            else
            {
                foreach (var child in InternalChildren)
                {
                    var measure = child.Measure(null, new Vector2(0, alottedSize.Y));

                    if (lineHeight + measure.Y + VerticalSpacing > alottedSize.Y && alottedSize.Y > 0)
                    {
                        size.Y = Math.Max(size.Y, lineHeight);
                        lineHeight = measure.Y + VerticalSpacing;
                        size.X += lineWidth + HorizontalSpacing;
                        lineWidth = measure.X;
                    }
                    else
                    {
                        lineHeight += measure.Y + VerticalSpacing;
                        lineWidth = Math.Max(lineWidth, measure.X);
                    }
                }
                
                size.Y = Math.Max(size.Y, lineHeight);
                size.X += lineWidth;
            }
            
            return size;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            var x = (float) ContentRectangle.Left;
            var y = (float) ContentRectangle.Top;
            
            if (Direction == FlowDirection.TopDown)
            {
                var cx = x;
                var cy = y;
                var maxy = bounds.Bottom;
                var w = 0f;

                foreach (var child in InternalChildren)
                {
                    var measure = child.Measure();

                    if (cy + measure.Y + VerticalSpacing > maxy)
                    {
                        cy = y;
                        cx += w + HorizontalSpacing;
                        w = 0;
                    }

                    child.Layout(new Rectangle((int) cx, (int) cy, (int) measure.X, (int) measure.Y));
                    cy += measure.Y + VerticalSpacing;
                    w = Math.Max(w, measure.X);
                }
            }
            else if (Direction == FlowDirection.LeftToRight)
            {
                var cx = x;
                var cy = y;
                var maxx = bounds.Right;
                var h = 0f;

                foreach (var child in InternalChildren)
                {
                    var measure = child.Measure();

                    if (cx + measure.X + HorizontalSpacing > maxx)
                    {
                        cx = x;
                        cy += h + VerticalSpacing;
                        h = 0;
                    }

                    h = Math.Max(h, measure.Y);
                    child.Layout(new Rectangle((int) cx, (int) cy, (int) measure.X, (int) measure.Y));
                    cx += measure.X + HorizontalSpacing;
                }
            }
        }
    }
}