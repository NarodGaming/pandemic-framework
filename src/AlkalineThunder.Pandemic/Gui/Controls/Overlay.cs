using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A container that overlays all of it's children on-top of each other.
    /// </summary>
    [MarkupElement("overlay")]
    public class Overlay : ContainerControl
    {
        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var m = Vector2.Zero;

            foreach (var child in InternalChildren)
            {
                var cm = child.Measure(null, alottedSize);
                m.X = Math.Max(cm.X, m.X);
                m.Y = Math.Max(cm.Y, m.Y);
            }
            
            return m;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            foreach (var child in InternalChildren)
                child.Layout(bounds);
        }
    }
}