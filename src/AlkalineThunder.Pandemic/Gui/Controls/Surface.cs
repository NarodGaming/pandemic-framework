using System;
using AlkalineThunder.Pandemic.Gui.Markup;
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Controls
{
    [MarkupElement("surface")]
    public sealed class Surface : Control
    {
        private GameTime _gameTime;
        
        public class SurfaceDrawEventArgs : EventArgs
        {
            public SpriteRocket2D Renderer { get; }
            public GameTime GameTime { get; }
            public Rectangle BoundingBox { get; }
            
            public SurfaceDrawEventArgs(GameTime time, SpriteRocket2D renderer, Rectangle boundingBox)
            {
                GameTime = time;
                Renderer = renderer;
                BoundingBox = boundingBox;
            }
        }
        
        public event EventHandler<SurfaceDrawEventArgs> SurfaceDraw;

        [MarkupProperty("bg")] public ControlColor BackgroundColor { get; set; } = ControlColor.Default;

        protected override void OnUpdate(GameTime gameTime)
        {
            _gameTime = gameTime;
        }

        protected override void OnPaint(SpriteRocket2D renderer)
        {
            renderer.Begin();

            renderer.FillRectangle(BoundingBox, BackgroundColor.GetColor(this));
            
            SurfaceDraw?.Invoke(this, new SurfaceDrawEventArgs(_gameTime, renderer, BoundingBox));

            renderer.End();
        }
    }
}