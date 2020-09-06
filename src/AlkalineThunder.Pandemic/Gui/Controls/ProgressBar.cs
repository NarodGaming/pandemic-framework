using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A control that indicates progress.
    /// </summary>
    /// <remarks>
    /// If the opposite of "pro" is "con," does that mean that the opposite of "progress" is
    /// "congress?"
    /// </remarks>
    [MarkupElement("progress")]
    public sealed class ProgressBar : Control
    {
        /// <summary>
        /// Gets or sets the percentage of progress displayed in the control.
        /// </summary>
        [MarkupProperty("value")]
        public float Percentage { get; set; }
        
        /// <summary>
        /// Gets or sets the color of progress.
        /// </summary>
        [MarkupProperty("color")]
        public ControlColor ProgressColor { get; set; } = ControlColor.Primary;
        
        /// <summary>
        /// Gets or sets the background color of the progress bar.
        /// </summary>
        [MarkupProperty("bg")]
        public ControlColor BackgroundColor { get; set; } = ControlColor.Text;
        
        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            return new Vector2(0, Skin.LayoutInfo.ProgressBarHeight);
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var height = Skin.LayoutInfo.ProgressBarHeight;
            var bounds = ContentRectangle;
            var bgColor = BackgroundColor.GetColor(this);
            var fgColor = ProgressColor.GetColor(this);
            var barBounds = new Rectangle(bounds.Left, bounds.Top + ((bounds.Height - height) / 2), bounds.Width,
                height);
            var progressBounds = new Rectangle(barBounds.Left, barBounds.Top,
                (int) (barBounds.Width * MathHelper.Clamp(Percentage, 0, 1)), barBounds.Height);
            
            renderer.Begin();
            renderer.FillRectangle(barBounds, bgColor);
            renderer.FillRectangle(progressBounds, fgColor);
            renderer.End();
        }
    }
}