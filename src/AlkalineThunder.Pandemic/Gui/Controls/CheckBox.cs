using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A user interface element that represents a toggle-able value and can display a content element
    /// next to it.
    /// </summary>
    [MarkupElement("checkbox")]
    public sealed class CheckBox : ContentControl
    {
        private const int CheckSpacing = 6;
        private const int CheckSize = 20;
        
        /// <summary>
        /// Gets or sets a value indicating the current state of the check box.
        /// </summary>
        [MarkupProperty("state")]
        public CheckState CheckState { get; set; } = CheckState.Unchecked;
        
        /// <summary>
        /// Gets or sets the color of the check box.
        /// </summary>
        public ControlColor CheckColor { get; set; } = ControlColor.Text;
        
        /// <summary>
        /// Gets a value indicating whether the check box is checked.
        /// </summary>
        public bool IsChecked => CheckState == CheckState.Checked;

        /// <summary>
        /// Creates a new instance of the <see cref="CheckBox"/> class.
        /// </summary>
        public CheckBox()
        {
            ContentVerticalAlignment = VerticalAlignment.Center;
            Padding = new Padding(4, 2);
        }

        /// <inheritdoc />
        protected override bool OnClick(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                if (IsChecked)
                {
                    CheckState = CheckState.Unchecked;
                }
                else
                {
                    CheckState = CheckState.Checked;
                }
                return base.OnClick(e) || true;
            }
            
            return base.OnClick(e);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            var childAlotted = new Vector2(Math.Max(alottedSize.X - (CheckSize + CheckSpacing), 0), alottedSize.Y);
            var baseMeasure = (Content?.Measure(null, childAlotted)).GetValueOrDefault();

            baseMeasure.X += CheckSize + CheckSpacing;
            baseMeasure.Y = Math.Max(CheckSize, baseMeasure.Y);

            return baseMeasure;
        }

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            bounds.X += CheckSize + CheckSpacing;
            bounds.Width -= CheckSize + CheckSpacing;
            base.Arrange(bounds);
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var checkColor = CheckColor.GetColor(this);

            var checkBounds = new Rectangle(ContentRectangle.Left,
                ContentRectangle.Top + ((ContentRectangle.Height - CheckSize) / 2), CheckSize, CheckSize);

            var checkTexture = Skin.Textures.CheckBoxUnchecked;

            if (CheckState == CheckState.Unknown)
                checkTexture = Skin.Textures.CheckBoxUnknown;
            else if (CheckState == CheckState.Checked)
                checkTexture = Skin.Textures.CheckBoxChecked;
            
            renderer.Begin();
            renderer.FillRectangle(checkBounds, checkColor, checkTexture);
            renderer.End();
        }
    }
}