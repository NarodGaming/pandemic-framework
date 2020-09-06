using System;
using System.Collections.Generic;
using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A user interface layout element similar to <see cref="StackPanel"/> but that allows its children to
    /// fill the remaining space of the panel.
    /// </summary>
    [MarkupElement("group")]
    public sealed class AdvancedStackPanel : ContainerControl
    {
        /// <summary>
        /// Represents the name of the attached fill property.
        /// </summary>
        [MarkupType(typeof(float))]
        public static readonly string FillProperty = "fill";
        
        /// <summary>
        /// Represents the name of the attached auto-size proprty.
        /// </summary>
        [MarkupType(typeof(bool))]
        public static readonly string AutoSizeProperty = "autoSize";
        
        private int _spacing;
        private Orientation _orientation = Orientation.Vertical;

        /// <summary>
        /// Gets or sets a value representing the amount of spacing between each child.
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

        /// <summary>
        /// Gets or sets a value indicating the <see cref="Orientation"/> in which each child is stacked.
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
        /// Gets a value representing the fill percentage of a child.
        /// </summary>
        /// <param name="control">The child to read the property from.</param>
        /// <returns>A percentage representing how much of the remaining space the child will fill.</returns>
        public float GetFill(Control control)
        {
            if (control.HasAttachedProperty<float>(FillProperty))
                return MathHelper.Clamp(GetAttachedProperty<float>(FillProperty), 0, 1);
            return 1.0f;
        }

        /// <summary>
        /// Gets a value indicating whether the given child will auto-size within the advanced stack panel.
        /// </summary>
        /// <param name="control">The child to read the property from.</param>
        /// <returns>A value indicating whether the child is to be auto-sized.</returns>
        public bool GetAutoSize(Control control)
        {
            if (control.HasAttachedProperty<bool>(AutoSizeProperty))
                return control.GetAttachedProperty<bool>(AutoSizeProperty);
            return true;
        }

        /// <summary>
        /// Sets a value on a child indicating whether it should be auto-sized in an advanced stack panel.
        /// </summary>
        /// <param name="control">The child to set the property on.</param>
        /// <param name="value">The value of the property.</param>
        public void SetAutoSize(Control control, bool value)
        {
            control.SetAttachedProperty(AutoSizeProperty, value);
        }

        /// <summary>
        /// Sets a value on a child indicating how much space the child should fill in an advanced stack panel.
        /// </summary>
        /// <param name="control">The control to set the property on.</param>
        /// <param name="amount">A percentage indicating how much space the child should take yp.</param>
        public void SetFill(Control control, float amount)
        {
            control.SetAttachedProperty(FillProperty, MathHelper.Clamp(amount, 0, 1));
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var size = Vector2.Zero;

            // This has to be done in two passes.
            // First we measure the controls that are auto-sized. That's pass 1.
            // Then, if we have an alotted size in the direction of our orientation, we need to figure out
            // how much of the remaining space of that should be taken up by our filling controls.
            //
            // All of that will be our measurement.

            // But first, spacing.
            var sp = Spacing * (InternalChildren.Count - 1);
            
            if (_orientation == Orientation.Vertical)
            {
                var space = Math.Max(0, alottedSize.Y - sp);
                size.Y += sp;
                
                // Pass 1: Auto-sized children.
                var fillSegments = 0;
                foreach (var child in InternalChildren)
                {
                    if (GetAutoSize(child))
                    {
                        var measurement = child.Measure(null, new Vector2(alottedSize.X, 0));
                        size.X = Math.Max(size.X, measurement.X);
                        size.Y += measurement.Y;
                        space = Math.Max(0, space - measurement.Y);
                    }
                    else
                    {
                        fillSegments++;
                    }
                }
                
                // Pass 2: Filled controls.
                if (fillSegments > 0)
                {
                    var segment = space / fillSegments;
                    foreach (var child in InternalChildren)
                    {
                        if (!GetAutoSize(child))
                        {
                            var fill = GetFill(child);
                            size.Y += segment * fill;
                            size.X = Math.Max(size.X, child.Measure(null, new Vector2(alottedSize.X, 0)).X);
                        }
                    }
                }
            }
            else
            {
                var space = Math.Max(0, alottedSize.X - sp);
                size.X += sp;
                
                // Pass 1: Auto-sized children.
                var fillSegments = 0;
                foreach (var child in InternalChildren)
                {
                    if (GetAutoSize(child))
                    {
                        var measurement = child.Measure(null, new Vector2(0, alottedSize.Y));
                        size.Y = Math.Max(size.Y, measurement.Y);
                        size.X += measurement.X;
                        space = Math.Max(0, space - measurement.X);
                    }
                    else
                    {
                        fillSegments++;
                    }
                }
                
                // Pass 2: Filled controls.
                if (fillSegments > 0)
                {
                    var segment = space / fillSegments;
                    foreach (var child in InternalChildren)
                    {
                        if (!GetAutoSize(child))
                        {
                            var fill = GetFill(child);
                            size.X += segment * fill;
                            size.Y = Math.Max(size.Y, child.Measure(null, new Vector2(0, alottedSize.Y)).Y);
                        }
                    }
                }

            }
            
            return size;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            var rects = new List<Rectangle>();

            // Pass 1: Count up the amount of filling children.
            // Also sum up the space that aytosized children take
            var fillCount = 0;
            var autoSizeAmount = 0f;
            foreach (var child in InternalChildren)
            {
                if (!GetAutoSize(child))
                    fillCount++;
                else
                {
                    var m = child.Measure();
                    if (Orientation == Orientation.Vertical)
                    {
                        autoSizeAmount += m.Y;
                    }
                    else
                    {
                        autoSizeAmount += m.X;
                    }
                }
            }
            
            // Pass 2: calculate layout rectangles.
            if (Orientation == Orientation.Vertical)
            {
                var space = Math.Max(0, bounds.Height - (Spacing * (InternalChildren.Count - 1)) - autoSizeAmount);
                var top = bounds.Top;

                for (var i = 0; i < InternalChildren.Count; i++)
                {
                    var child = InternalChildren[i];
                    var fill = GetFill(child);
                    var autoSize = GetAutoSize(child);
                    var measure = child.Measure();

                    if (autoSize)
                    {
                        rects.Add(new Rectangle(bounds.Left, top, bounds.Width, (int) measure.Y));
                        top += (int) measure.Y + Spacing;
                    }
                    else
                    {
                        var segHeight = (space / fillCount) * fill;
                        rects.Add(new Rectangle(bounds.Left, top, bounds.Width, (int) segHeight));
                        top += (int) segHeight + Spacing;
                    }
                }
            }
            else
            {
                var space = Math.Max(0, bounds.Width - (Spacing * (InternalChildren.Count - 1)) - autoSizeAmount);
                var left = bounds.Left;

                for (var i = 0; i < InternalChildren.Count; i++)
                {
                    var child = InternalChildren[i];
                    var fill = GetFill(child);
                    var autoSize = GetAutoSize(child);
                    var measure = child.Measure();

                    if (autoSize)
                    {
                        rects.Add(new Rectangle(left, bounds.Top, (int) measure.X, bounds.Height));
                        left += (int) measure.X + Spacing;
                    }
                    else
                    {
                        var segWidth = (space / fillCount) * fill;
                        rects.Add(new Rectangle(left, bounds.Top, (int) segWidth, bounds.Height));
                        left += (int) segWidth + Spacing;
                    }
                }
            }
            
            // Pass 3: Perform layout.
            for (var i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                child.Layout(rects[i]);
            }
        }
    }
}