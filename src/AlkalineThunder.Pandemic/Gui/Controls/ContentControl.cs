using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// Provides the base functionality for all user interface elements that can contain a single content element.
    /// </summary>
    public abstract class ContentControl : Control
    {
        private HorizontalAlignment _contentHAlign = HorizontalAlignment.Stretch;
        private VerticalAlignment _contentVAlign = VerticalAlignment.Stretch;

        /// <summary>
        /// Gets or sets the horizontal alignment of the content within this control.
        /// </summary>
        [MarkupProperty("content-horizontal-align")]
        public HorizontalAlignment ContentHorizontalAlignment
        {
            get => _contentHAlign;
            set
            {
                if (_contentHAlign != value)
                {
                    _contentHAlign = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the content within this control.
        /// </summary>
        [MarkupProperty("content-vertical-align")]
        public VerticalAlignment ContentVerticalAlignment
        {
            get => _contentVAlign;
            set
            {
                if (_contentVAlign != value)
                {
                    _contentVAlign = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the content of this control.
        /// </summary>
        public Control Content
        {
            get
            {
                if (InternalChildren.Count > 0)
                    return InternalChildren[0];
                return null;
            }
            set
            {
                if (value != Content)
                {
                    if (InternalChildren.Count > 0)
                    {
                        InternalChildren.Remove(Content);
                    }

                    if (value != null)
                    {
                        InternalChildren.Add(value);
                    }

                    OnContentChanged(value);
                }
            }
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            if (Content != null)
                return Content.Measure(null, alottedSize);
            else
                return Vector2.Zero;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            if (Content != null)
            {
                var measure = Content.Measure();
                var childBounds = LayoutUtils.CalculateBoundingBox(bounds, ContentHorizontalAlignment,
                    ContentVerticalAlignment, measure);

                Content.Layout(childBounds);
            }
        }

        /// <summary>
        /// Called when the control's content is changed.
        /// </summary>
        /// <param name="content">The new content of the control.</param>
        protected virtual void OnContentChanged(Control content)
        {
            
        }
    }
}
