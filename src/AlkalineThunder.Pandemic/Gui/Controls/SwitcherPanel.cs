using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A control that contains multiple children but only displays one on-screen at a time.
    /// </summary>
    [MarkupElement("switcher")]
    public sealed class SwitcherPanel : ContentControl
    {
        private int _activeIndex;

        /// <summary>
        /// Gets or sets the active control of the switcher.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">The specified index does not refer to a control inside the switcher's child list.</exception>
        public int ActiveIndex
        {
            get => _activeIndex;
            set
            {
                if (_activeIndex != value)
                {
                    if (InternalChildren.Count > 0)
                    {
                        if (value < 0 || value >= InternalChildren.Count)
                            throw new IndexOutOfRangeException();

                        _activeIndex = value;
                        InvalidateMeasure();
                    }
                    else
                    {
                        if (value != 0)
                            throw new IndexOutOfRangeException();

                        _activeIndex = value;
                        InvalidateMeasure();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the switcher's active control.
        /// </summary>
        public Control ActiveItem => (_activeIndex > 0 && _activeIndex < InternalChildren.Count)
            ? InternalChildren[_activeIndex]
            : null;

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            if (ActiveItem != null)
                return ActiveItem.Measure();

            return Vector2.Zero;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            foreach (var child in InternalChildren)
            {
                if (child == ActiveItem)
                {
                    child.Layout(bounds);
                }
                else
                {
                    child.Layout(Rectangle.Empty);
                }
            }
        }
    }
}