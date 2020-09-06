using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Rendering;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A user interface element with a background color that can contain a single aligned child.
    /// </summary>
    [MarkupElement("box")]
    public class Box : ContentControl
    {
        /// <summary>
        /// Gets or sets the background color of the box.
        /// </summary>
        [MarkupProperty("bg")]
        public ControlColor BackgroundColor { get; set; } = ControlColor.Default;
        
        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var color = BackgroundColor.GetColor(this);

            renderer.Begin();

            renderer.FillRectangle(BoundingBox, color);

            renderer.End();
        }
    }
}