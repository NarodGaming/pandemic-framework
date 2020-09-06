using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Rendering;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// An image display element that can be tinted and treated like text.
    /// </summary>
    [MarkupElement("icon")]
    public class Icon : PictureBox
    {
        /// <summary>
        /// Gets or sets the color of the icon.
        /// </summary>
        [MarkupProperty("color")]
        public ControlColor Color { get; set; } = ControlColor.Text;

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var color = Color.GetColor(this);
            if (Image != null)
            {
                renderer.Begin();
                renderer.FillRectangle(ContentRectangle, color, Image);
                renderer.End();
            }

        }
    }
}