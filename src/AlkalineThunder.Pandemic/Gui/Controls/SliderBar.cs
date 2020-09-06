using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Input;
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A control that allows the user to choose a value within a range of values.
    /// </summary>
    [MarkupElement(("slider"))]
    public sealed class SliderBar : Control
    {
        private float _value;
        private float _sliderRadius = 8;
        private bool _hovered;
        private bool _pressed;
        
        /// <summary>
        /// Gets or sets the color of the slider's ball, or value indication.
        /// </summary>
        [MarkupProperty("color")]
        public ControlColor ValueColor { get; set; } = ControlColor.Primary;
        
        /// <summary>
        /// Gets or sets the value of the slider bar.
        /// </summary>
        [MarkupProperty("value")]
        public float Value
        {
            get => _value;
            set
            {
                if (Math.Abs(_value - value) > .01f)
                {
                    _value = MathHelper.Clamp(value, 0, 1);
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SliderBar"/> control.
        /// </summary>
        public SliderBar()
        {
            MinWidth = (_sliderRadius * 2) + 100;
        }
        
        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            return new Vector2(_sliderRadius * 2, _sliderRadius * 2);
        }

        /// <inheritdoc />
        protected override bool OnMouseEnter(MouseMoveEventArgs e)
        {
            _hovered = true;
            return base.OnMouseEnter(e);
        }

        /// <inheritdoc />
        protected override bool OnMouseLeave(MouseMoveEventArgs e)
        {
            _hovered = false;
            return base.OnMouseLeave(e);
        }

        /// <inheritdoc />
        protected override bool OnMouseMove(MouseMoveEventArgs e)
        {
            if (_pressed)
            {
                var min = ContentRectangle.Left + _sliderRadius;
                var max = ContentRectangle.Right - _sliderRadius;
                var w = max - min;
                var v = (e.X - min) / w;
                _value = MathHelper.Clamp(v, 0, 1);
            }
            return base.OnMouseMove(e);
        }

        /// <inheritdoc />
        protected override bool OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left)
            {
                var min = ContentRectangle.Left + _sliderRadius;
                var max = ContentRectangle.Right - _sliderRadius;
                var w = max - min;
                var v = (e.X - min) / w;
                _value = MathHelper.Clamp(v, 0, 1);

                _pressed = true;
            }

            return base.OnMouseDown(e);
        }

        /// <inheritdoc />
        protected override bool OnMouseUp(MouseButtonEventArgs e)
        {
            if (_pressed)
            {
                _pressed = false;
            }
            
            return base.OnMouseUp(e);
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var fg = ValueColor.GetColor(this);

            if (_pressed)
                fg = fg.Darken(0.15f);
            else if (_hovered)
                fg = fg.Lighten(0.15f);
            
            var sliderBounds = new Rectangle(ContentRectangle.Left,
                ContentRectangle.Top + ((ContentRectangle.Height - 1) / 2), ContentRectangle.Width, 1);

            var sliderRangeMin = sliderBounds.Left + _sliderRadius;
            var sliderRangeMax = sliderBounds.Right - _sliderRadius;

            var sliderBallX = MathHelper.Lerp(sliderRangeMin, sliderRangeMax, Value);
            var sliderY = sliderBounds.Center.Y;

            renderer.Begin();
            renderer.FillRectangle(sliderBounds, fg);
            renderer.FillCircle(new Vector2(sliderBallX, sliderY), _sliderRadius, fg);
            renderer.End();
        }
    }
}