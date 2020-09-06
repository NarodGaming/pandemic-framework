using System;
using System.Linq;
using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A container control that can dock its children to the edges of the container's bounds.
    /// </summary>
    [MarkupElement("dockPanel")]
    public sealed class DockPanel : ContainerControl
    {
        /// <summary>
        /// Represents the name of the attached property given to all <see cref="DockPanel"/> children in which
        /// the child's <see cref="Dock"/> is stored.
        /// </summary>
        [MarkupType(typeof(Dock))]
        public const string DockProperty = "dock";

        /// <summary>
        /// Represents the way in which a <see cref="DockPanel"/> child should be docked.
        /// </summary>
        public enum Dock
        {
            /// <summary>
            /// The child should be docked to the top edge of the container.
            /// </summary>
            Top,
            
            /// <summary>
            /// The child should be docked to the left side of the container.
            /// </summary>
            Left,
            
            /// <summary>
            /// The child should be docked to the bottom of the container.
            /// </summary>
            Bottom,
            
            /// <summary>
            /// the child should be docked to the right side of the container.
            /// </summary>
            Right
        }

        /// <summary>
        /// Sets the dock of a control.
        /// </summary>
        /// <param name="child">The control to dock.</param>
        /// <param name="dock">The control's new dock style.</param>
        public void SetDock(Control child, Dock dock)
        {
            child?.SetAttachedProperty(DockProperty, dock);
        }

        /// <summary>
        /// Gets the dock style of a control.
        /// </summary>
        /// <param name="child">The control to read the dock style from.</param>
        /// <returns>The dock style of the control.</returns>
        /// <remarks>
        /// FREEEEEEEEEEEDOM YEAHHHHHHHHHHHHHHHHHHHHHHHHH
        /// </remarks>
        public Dock GetDock(Control child)
        {
            if (child != null && child.HasAttachedProperty<Dock>(DockProperty))
            {
                return child.GetAttachedProperty<Dock>(DockProperty);
            }
            else
            {
                return Dock.Top;
            }
        }
        
        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            if (InternalChildren.Count > 0)
            {
                // DockPanel measurement requires three passes.
                // Pass 1:
                //    Measure all children but the last, so we know how much space
                //    the children docked to the edges take up.
                // Pass 2:
                //    Measure the last child, using the measurement of pass 1 as the
                //    reserved area.  This prevents the last child from exceeding the
                //    remaining bounds of the DockPanel, and allows TextBlocks and other
                //    controls to reliably and accurately measure in certain scenarios where,
                //    for example, an ancestor of the Dock Panel has a fixed/max size.
                // Pass 3:
                //    Finally, we iterate through all but the last child to pad the measurement
                //    of the last child with the measurements of each dock panel edge.  The resulting
                //    size is the measurement of the dock panel itself.
                
                // Pass 1: measure each edge.
                var prelimSize = Vector2.Zero;

                for (var i = 0; i < InternalChildren.Count - 1; i++)
                {
                    var child = InternalChildren[i];
                    var measure = child.Measure();
                    var dock = GetDock(child);

                    switch (dock)
                    {
                        case Dock.Left:
                        case Dock.Right:
                            prelimSize.X += measure.X;
                            break;
                        case Dock.Top:
                        case Dock.Bottom:
                            prelimSize.Y += measure.Y;
                            break;
                    }
                }

                var alottment = new Vector2(
                    Math.Max(0, alottedSize.X - prelimSize.X),
                    Math.Max(0, alottedSize.Y - prelimSize.Y)
                );
                
                // Pass 2: measure the last child.
                var size = InternalChildren.Last().Measure(null, alottment);

                // Pass 3: compute the final size.
                for (var i = 0; i < InternalChildren.Count - 1; i++)
                {
                    var child = InternalChildren[i];
                    var measure = child.Measure();
                    var dock = GetDock(child);

                    switch (dock)
                    {
                        case Dock.Left: 
                        case Dock.Right:
                            size.Y = Math.Max(measure.Y, size.Y);
                            size.X += measure.X;
                            break;
                        case Dock.Top:
                        case Dock.Bottom:
                            size.X = Math.Max(measure.X, size.X);
                            size.Y += measure.Y;
                            break;
                    }
                }

                return size;
            }
            else
            {
                return Vector2.Zero;
            }
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            int i = 0;
            
            foreach (var child in InternalChildren)
            {
                var size = child.Measure();
                var dockStyle = child.GetAttachedProperty<Dock>(DockProperty);

                if (i == InternalChildren.Count - 1)
                {
                    child.Layout(bounds);
                }
                else
                {
                    switch (dockStyle)
                    {
                        case Dock.Top:
                            child.Layout(new Rectangle(bounds.Left, bounds.Top, bounds.Width, (int) size.Y));
                            bounds.Y += (int) size.Y;
                            bounds.Height -= (int) size.Y;
                            break;
                        case Dock.Bottom:
                            child.Layout(new Rectangle(bounds.Left, bounds.Bottom - (int) size.Y, bounds.Width,
                                (int) size.Y));

                            bounds.Height -= (int) size.Y;
                            break;
                        case Dock.Left:
                            child.Layout(new Rectangle(bounds.Left, bounds.Top, (int) size.X, bounds.Height));
                            bounds.X += (int) size.X;
                            bounds.Width -= (int) size.X;
                            break;
                        case Dock.Right:
                            child.Layout(
                                new Rectangle(bounds.Right - (int) size.X, bounds.Top, (int) size.X, bounds.Height));

                            bounds.Width -= (int) size.X;
                            break;

                    }
                }

                i++;
            }
        }
    }
}