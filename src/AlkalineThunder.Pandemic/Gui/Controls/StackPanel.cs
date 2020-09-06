using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A simple layout container that stacks its children in a given orientation.
    /// </summary>
    [MarkupElement("stackPanel")]
    public sealed class StackPanel : ContainerControl
    {
        private Orientation _orientation = Orientation.Vertical;
        private int _spacing;

        /// <summary>
        /// Gets or sets the orientation in which the container's children are stacked.
        /// </summary>
        [MarkupProperty("orientation")]
        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Gets or sets the spacing between each item.
        /// </summary>
        [MarkupProperty("spacing")]
        public int Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    InvalidateMeasure();
                }
            }
        }
        
        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var measurement = Vector2.Zero;

            if (Orientation == Orientation.Horizontal)
            {
                foreach (var child in Children)
                {
                    var cMeasurement = child.Measure(null, new Vector2(0, alottedSize.Y));

                    measurement.X += cMeasurement.X;
                    measurement.Y = Math.Max(cMeasurement.Y, measurement.Y);
                }

                measurement.X += (Spacing * (InternalChildren.Count - 1));
            }
            else
            {
                foreach (var child in Children)
                {
                    var cMeasurement = child.Measure(null, new Vector2(alottedSize.X, 0));

                    measurement.Y += cMeasurement.Y;
                    measurement.X = Math.Max(cMeasurement.X, measurement.X);
                }
                
                measurement.Y += (Spacing * (InternalChildren.Count - 1));
            }
            
            return measurement;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            if (Orientation == Orientation.Horizontal)
            {
                foreach (var child in InternalChildren)
                {
                    var m = child.Measure();
                    var childBounds = new Rectangle(bounds.Left, bounds.Top, (int) m.X, bounds.Height);
                    child.Layout(childBounds);
                    bounds.X += childBounds.Width + Spacing;
                    bounds.Width -= (childBounds.Width + Spacing);
                }
            }
            else
            {
                foreach (var child in InternalChildren)
                {
                    var m = child.Measure();
                    var childBounds = new Rectangle(bounds.Left, bounds.Top, bounds.Width, (int) m.Y);
                    child.Layout(childBounds);
                    bounds.Y += childBounds.Height + Spacing;
                    bounds.Height -= (childBounds.Height + Spacing);
                }
            }
        }
    }
}