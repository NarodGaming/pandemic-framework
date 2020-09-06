using AlkalineThunder.Pandemic.Gui.Markup;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A blank control that's simply used to create whitespace.
    /// </summary>
    [MarkupElement("spacer")]
    public class Spacer : Control
    {
        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            return Vector2.Zero;
        }
    }
}