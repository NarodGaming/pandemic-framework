using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A content control that will blur whatever content is drawn behind it.
    /// </summary>
    [MarkupElement("blur")]
    public class BackgroundBlur : ContentControl
    {
        private bool _blurInvalid = true;
        private float _radius = 0.05f;
        private RenderTarget2D _rt;
        private RenderTarget2D _vertRt;
        
        /// <summary>
        /// Gets or sets the color of the blur overlay.
        /// </summary>
        [MarkupProperty("bg")]
        public ControlColor BackgroundColor { get; set; } = ControlColor.Default;

        /// <summary>
        /// Gets or sets the radius of the blur effect.
        /// </summary>
        public float Radsius
        {
            get => _radius;
            set
            {
                if (Math.Abs(_radius - value) > .01f)
                {
                    _radius = value;
                    InvalidateMeasure();
                }
            }
        }

        private void InvalidateBlur()
        {
            _blurInvalid = true;

            var fb = GameLoop.CurrentGame.GetFrameBuffer();
            
            if (_rt == null)
            {
                _rt = new RenderTarget2D(fb.GraphicsDevice, fb.Width, fb.Height, false, fb.Format, DepthFormat.None, 0,
                    RenderTargetUsage.PreserveContents);
            }
            else if (_rt.Width != fb.Width || _rt.Height != fb.Height)
            {
                _rt.Dispose();
                _rt = new RenderTarget2D(fb.GraphicsDevice, fb.Width, fb.Height, false, fb.Format, DepthFormat.None, 0,
                    RenderTargetUsage.PreserveContents);
            }
            
            if (_vertRt == null)
            {
                _vertRt = new RenderTarget2D(fb.GraphicsDevice, fb.Width, fb.Height, false, fb.Format, DepthFormat.None, 0,
                    RenderTargetUsage.PreserveContents);
            }
            else if (_vertRt.Width != fb.Width || _vertRt.Height != fb.Height)
            {
                _vertRt.Dispose();
                _vertRt = new RenderTarget2D(fb.GraphicsDevice, fb.Width, fb.Height, false, fb.Format, DepthFormat.None, 0,
                    RenderTargetUsage.PreserveContents);
            }
        }
        
        /// <summary>
        /// Gets or sets the opacity of the blur overlay.
        /// </summary>
        [MarkupProperty("opacity")]
        public float Opacity { get; set; } = 0.5f;

        /// <inheritdoc />
        protected override void Arrange(Rectangle bounds)
        {
            InvalidateBlur();
            base.Arrange(bounds);
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            var settings = SceneSystem.GetModule<SettingsService>();
            
            if (settings.EnableTerminalTransparency && settings.EnableBlurs)
            {
                var clipRect = renderer.ClippingRectangle;
                var framebuffer = GameLoop.CurrentGame.GetFrameBuffer();

                if (_blurInvalid)
                {
                    renderer.ClippingRectangle = framebuffer.Bounds;
                    renderer.GraphicsDevice.SetRenderTarget(_rt);

                    GameLoop.CurrentGame.Blur.Parameters["radialBlurLength"].SetValue(_radius);
                    renderer.Begin(GameLoop.CurrentGame.Blur);
                    renderer.FillRectangle(new Rectangle(0, 0, framebuffer.Width, framebuffer.Height), Color.White,
                        framebuffer);
                    renderer.End();

                    _blurInvalid = false;
                }

                renderer.GraphicsDevice.SetRenderTarget((RenderTarget2D) framebuffer);
                renderer.ClippingRectangle = clipRect;
                renderer.Begin();
                renderer.FillRectangle(framebuffer.Bounds, Color.White, _rt);
                renderer.FillRectangle(BoundingBox, BackgroundColor.GetColor(this) * Opacity);
                renderer.End();
            }
            else
            {
                renderer.Begin();
                renderer.FillRectangle(BoundingBox, BackgroundColor.GetColor(this) * (settings.EnableTerminalTransparency ? Opacity : 1));
                renderer.End();
            }
        }
    }
}