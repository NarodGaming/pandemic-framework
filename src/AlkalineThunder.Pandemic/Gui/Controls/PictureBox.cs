using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// A simple way to display a picture in a user interface.
    /// </summary>
    [MarkupElement("image")]
    public class PictureBox : Control
    {
        private Texture2D _image;
        private bool _maintainAspectRatio = true;

        /// <summary>
        /// Gets or sets whether the image's aspect ratio is maintained.
        /// </summary>
        [MarkupProperty("maintain-aspect-ratio")]
        public bool MaintainAspectRatio
        {
            get => _maintainAspectRatio;
            set
            {
                if (_maintainAspectRatio != value)
                {
                    _maintainAspectRatio = value;
                    InvalidateMeasure();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the image to be displayed.
        /// </summary>
        [MarkupProperty("src")]
        public Texture2D Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// Sets the image to an in-game resource, file, or web URL.
        /// </summary>
        /// <param name="src">The path to the image to use.</param>
        public void SetImageSource(string src)
        {
            Image = GameLoop.LoadTexture(src);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureOverride(Vector2 alottedSize)
        {
            if (_image != null)
            {
                if (_maintainAspectRatio)
                {
                    var aspectRatio = _image.Width / (float) _image.Height;

                    var w = (float) _image.Width;
                    var h = (float) _image.Height;

                    if (alottedSize.X > 0)
                    {
                        w = alottedSize.X;
                        h = w / aspectRatio;
                    }
                    else if (alottedSize.Y > 0)
                    {
                        h = alottedSize.Y;
                        w = h * aspectRatio;
                    }
                    
                    return new Vector2(w, h);
                }
                else
                {
                    return new Vector2(_image.Width, _image.Height);
                }
            }

            return Vector2.Zero;
        }

        /// <inheritdoc />
        protected override void OnPaint(SpriteRocket2D renderer)
        {
            if (_image != null)
            {
                renderer.Begin();
                renderer.FillRectangle(ContentRectangle, Color.White, _image);
                renderer.End();
            }
        }
    }
}