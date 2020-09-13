using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace AlkalineThunder.Pandemic.Rendering
{
    /// <summary>
    /// The Sprite Rocket is an extremely enhanced version of the MonoGame Sprite Batch with heaps more
    /// options for renderable polygons.
    /// </summary>
    public sealed class SpriteRocket2D : IGlyphRenderer
    {
        private Effect _effect;
        private bool _running;
        private BlendState _blendState;
        private SamplerState _samplerState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;
        private List<RenderItem> _batch = new List<RenderItem>();
        private Texture2D _blankTexture;
        private SpriteEffect _spriteEffect;
        private Matrix _transformMatrix;
        
        /// <summary>
        /// Gets the graphics rendering device that this renderer will draw to.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; }
        
        /// <inheritdoc />
        public void Draw(Texture2D texture, Rectangle destRect, Rectangle sourceRect, Color color, float rotation, Vector2 origin,
            SpriteEffects effect, float depth)
        {
            var item = MakeRenderItem(texture);

            var texRect = texture.Bounds;
            
            var texCoordTl = LinearMap(new Vector2(sourceRect.Left, sourceRect.Top), texRect, RectangleF.Unit);
            var texCoordTr = LinearMap(new Vector2(sourceRect.Right, sourceRect.Top), texRect, RectangleF.Unit);
            var texCoordBl = LinearMap(new Vector2(sourceRect.Left, sourceRect.Bottom), texRect, RectangleF.Unit);
            var texCoordBr = LinearMap(new Vector2(sourceRect.Right, sourceRect.Bottom), texRect, RectangleF.Unit);

            var v1 = item.AddVertex(destRect.Location.ToVector2(), color, texCoordTl);
            var v2 = item.AddVertex(new Vector2(destRect.Right, destRect.Top), color, texCoordTr);
            var v3 = item.AddVertex(new Vector2(destRect.Left, destRect.Bottom), color, texCoordBl);
            var v4 = item.AddVertex(new Vector2(destRect.Right, destRect.Bottom), color, texCoordBr);

            item.AddIndex(v1);
            item.AddIndex(v2);
            item.AddIndex(v3);
            item.AddIndex(v2);
            item.AddIndex(v3);
            item.AddIndex(v4);
        }

        /// <summary>
        /// Gets or sets a value representing the renderer's clipping boundaries.  Anything rendered outside these bounds
        /// will be clipped away.
        /// </summary>
        public Rectangle ClippingRectangle { get; set; } = Rectangle.Empty;
        
        /// <summary>
        /// Creates a new instance of the <see cref="SpriteRocket2D"/> class.
        /// </summary>
        /// <param name="device">The graphics device to render all polygons to.</param>
        public SpriteRocket2D(GraphicsDevice device)
        {
            GraphicsDevice = device ?? throw new ArgumentNullException(nameof(device));
            GameUtils.Log("Sprite Rocket is preparing for lift-off.");

            _blankTexture = new Texture2D(device, 1, 1);
            _blankTexture.SetData(new[] { 0xffffffff });

            _spriteEffect = new SpriteEffect(device);
        }

        /// <summary>
        /// Begins a new batch of polygons using the given settings for rendering.
        /// </summary>
        /// <param name="blendState">The blend state configuration for this batch.</param>
        /// <param name="samplerState">The sampler configuration for this batch.</param>
        /// <param name="depthStencilState">The depth/stencil configuration for this batch.</param>
        /// <param name="rasterizerState">The rasterizer configuration for this batch.</param>
        /// <exception cref="InvalidOperationException">A batch has already begun and hasn't been ended yet.</exception>
        public void Begin(BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
        {
            if (_running)
            {
                throw new InvalidOperationException("Cannot begin a new batch of sprites as the current batch has not been ended.");
            }

            _running = true;
            _blendState = blendState;
            _samplerState = samplerState;
            _depthStencilState = depthStencilState;

            if (ClippingRectangle.IsEmpty)
            {
                _rasterizerState = rasterizerState;
            }
            else
            {
                GraphicsDevice.ScissorRectangle = ClippingRectangle;

                _rasterizerState = new RasterizerState
                {
                    CullMode = rasterizerState.CullMode,
                    DepthBias = rasterizerState.DepthBias,
                    DepthClipEnable = rasterizerState.DepthClipEnable,
                    FillMode = rasterizerState.FillMode,
                    MultiSampleAntiAlias = rasterizerState.MultiSampleAntiAlias,
                    ScissorTestEnable = true,
                    SlopeScaleDepthBias = rasterizerState.SlopeScaleDepthBias
                };

            }

            _batch.Clear();

            _effect = null;
        }

        /// <summary>
        /// Begins a new batch of polygons.
        /// </summary>
        public void Begin()
        {
            Begin(BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone);
        }

        /// <summary>
        /// Begins a new batch of polygons, using the given effect.
        /// </summary>
        /// <param name="effect">A pixel shader effect to apply to all polygons in the batch.</param>
        public void Begin(Effect effect)
        {
            Begin();
            _effect = effect;
        }

        private RenderItem MakeRenderItem(Texture2D texture)
        {
            if (_running)
            {
                var tex = texture ?? _blankTexture;

                if (_batch.Count > 0)
                {
                    var last = _batch[_batch.Count - 1];
                    if (last.Texture == tex) return last;
                }

                var newBatchItem = new RenderItem();
                newBatchItem.Texture = tex;
                _batch.Add(newBatchItem);
                return newBatchItem;
            }
            else
            {
                throw new InvalidOperationException("You must call Begin() before you do this.");
            }
        }

        private Vector2 GetRealSize()
        {
            if (GraphicsDevice.RenderTargetCount > 0)
            {
                var rt = GraphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D;
                return new Vector2(rt.Width, rt.Height);
            }

            return new Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight);
        }
        
        /// <summary>
        /// Ends the current batch and draws all polygons to the screen.
        /// </summary>
        /// <exception cref="InvalidOperationException">A batch hasn't begun yet.</exception>
        public void End()
        {
            if (_running)
            {
                if (GameUtils.BaseResolution != null)
                {
                    var res = GameUtils.BaseResolution.GetValueOrDefault();
                    var real = GetRealSize();
                        
                    var scaleHeight = real.Y / res.Y;
                    var scaleWidth = real.X / res.X;

                    Matrix.CreateScale(scaleWidth, scaleHeight, 1, out _transformMatrix);

                    _spriteEffect.TransformMatrix = _transformMatrix;
                }
                else
                {
                    _spriteEffect.TransformMatrix = null;
                }

                GraphicsDevice.BlendState = _blendState;
                GraphicsDevice.RasterizerState = _rasterizerState;
                GraphicsDevice.SamplerStates[0] = _samplerState;
                GraphicsDevice.DepthStencilState = _depthStencilState;

                while (_batch.Count > 0)
                {
                    var item = _batch[0];
                    var vbo = item.Vertices;
                    var ibo = item.IndexBuffer;
                    var tex = item.Texture;

                    var pCount = item.Triangles;

                    if (pCount >= 1)
                    {
                        _spriteEffect.CurrentTechnique.Passes[0].Apply();
                        if (_effect != null)
                            _effect.CurrentTechnique.Passes[0].Apply();
                        GraphicsDevice.Textures[0] = tex;
                        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vbo, 0, vbo.Length, ibo, 0,
                            pCount);
                    }

                    _batch.RemoveAt(0);
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot end the current batch as it has not yet begun.");
            }
            
            _running = false;
        }

        /// <summary>
        /// Draws a rectangular outline.
        /// </summary>
        /// <param name="bounds">The area to draw the outline in.</param>
        /// <param name="color">The color of the outline.</param>
        /// <param name="thickness">The width of each line.</param>
        public void DrawRectangle(Rectangle bounds, Color color, int thickness)
        {
            if (thickness < 1)
                return;

            if (color.A <= 0)
                return;

            if (bounds.IsEmpty)
                return;

            if (bounds.Width <= thickness * 2 || bounds.Height <= thickness * 2)
            {
                FillRectangle(bounds, color);
            }
            
            var left = new Rectangle(bounds.Left, bounds.Top, thickness, bounds.Height);
            var right = new Rectangle(bounds.Right - thickness, bounds.Top, thickness, bounds.Height);
            var top = new Rectangle(left.Right, bounds.Top, bounds.Width - (thickness * 2), thickness);
            var bottom = new Rectangle(top.Left, bounds.Bottom - thickness, top.Width, top.Height);
            
            FillRectangle(left, color);
            FillRectangle(top, color);
            FillRectangle(right, color);
            FillRectangle(bottom, color);
        }
        
        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">THe color of the rectangle.</param>
        /// <param name="texture">The texture to fill the area with.</param>
        public void FillRectangle(Rectangle rect, Color color, Texture2D texture = null)
        {
            var renderItem = MakeRenderItem(texture);

            // add the 4 vertices
            var tl = renderItem.AddVertex(new Vector2(rect.Left, rect.Top), color, TextureCoords.TopLeft);
            var tr = renderItem.AddVertex(new Vector2(rect.Right, rect.Top), color, TextureCoords.TopRight);
            var bl = renderItem.AddVertex(new Vector2(rect.Left, rect.Bottom), color, TextureCoords.BottomLeft);
            var br = renderItem.AddVertex(new Vector2(rect.Right, rect.Bottom), color, TextureCoords.BottomRight);

            // firsst triangle
            renderItem.AddIndex(tl);
            renderItem.AddIndex(tr);
            renderItem.AddIndex(bl);

            // second triangle
            renderItem.AddIndex(tr);
            renderItem.AddIndex(bl);
            renderItem.AddIndex(br);

            // And that's how you draw a rectangle with two triangles!
        }

        /// <summary>
        /// Fills a rectangle with the given brush.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="brush">The brush to use for the rectangle.</param>
        public void DrawBrush(Rectangle rect, Brush brush)
        {
            if (brush.BrushType == BrushType.Image)
            {
                FillRectangle(rect, brush.Color, brush.Texture);
            }
            else if (brush.BrushType == BrushType.Box || brush.BrushType == BrushType.Outline)
            {
                if (brush.BrushType == BrushType.Box && brush.Texture == null)
                {
                    FillRectangle(rect, brush.Color);
                }
                else if (brush.Texture != null && (brush.Margin.Horizontal >= Math.Min(brush.Texture.Width, rect.Width) || brush.Margin.Vertical >= Math.Min(brush.Texture.Height, rect.Height)))
                {
                    FillRectangle(rect, brush.Color, brush.Texture);
                }
                else
                {
                    var renderItem = MakeRenderItem(brush.Texture);

                    var leftEdgeWidth = Math.Max(brush.Margin.Left, 0);
                    var rightEdgeWidth = Math.Max(brush.Margin.Right, 0);
                    var topEdgeHeight = Math.Max(brush.Margin.Top, 0);
                    var bottomEdgeHeight = Math.Max(brush.Margin.Bottom, 0);

                    var leftEdgeInnerU = leftEdgeWidth / rect.Width;
                    var rightEdgeInnerU = 1 - (rightEdgeWidth / rect.Width);
                    var topEdgeInnerV = topEdgeHeight / rect.Height;
                    var bottomEdgeInnerV = 1 - (bottomEdgeHeight / rect.Height);

                    // Texture coordinates for the outer corners of the rectangle.
                    var texCoordTl = TextureCoords.TopLeft;
                    var texCoordTr = TextureCoords.TopRight;
                    var texCoordBr = TextureCoords.BottomRight;
                    var texCoordBl = TextureCoords.BottomLeft;
                    
                    // Texture coordinates for the inner corners of the rectangle.
                    var texCoordInnerTl = texCoordTl + new Vector2(leftEdgeInnerU, topEdgeInnerV);
                    var texCoordInnerTr = new Vector2(rightEdgeInnerU, topEdgeInnerV);
                    var texCoordInnerBr = new Vector2(rightEdgeInnerU, bottomEdgeInnerV);
                    var texCoordInnerBl = new Vector2(leftEdgeInnerU, bottomEdgeInnerV);

                    // From here we can figure out the tex coords for the missing corner areas.
                    // IDFK what to call these. My fucking fry is brained.
                    var texCoordTopLeftCornerTr = new Vector2(texCoordInnerTl.X, texCoordTl.Y);
                    var texCoordTopLeftCornerBl = new Vector2(texCoordTl.X, texCoordInnerTl.Y);

                    // Fry is still brianed.
                    var texCoordTopRightCornerTl = new Vector2(texCoordInnerTr.X, texCoordTr.Y);
                    var texCoordTopRightCornerBr = new Vector2(texCoordTr.X, texCoordInnerTr.Y);

                    // Why is there no hole in this wall?
                    var texCoordBottomLeftCornerTl = new Vector2(texCoordBl.X, texCoordInnerBl.Y);
                    var texCoordBottomLeftCornerBr = new Vector2(texCoordInnerBl.X, texCoordBl.Y);

                    // Abcdefghijklmnopqrstuvwxyzqwerty
                    var texCoordBottomRightCornerTr = new Vector2(texCoordBr.X, texCoordInnerBr.Y);
                    var texCoordBottomRightCornerBl = new Vector2(texCoordInnerBr.X, texCoordBr.Y);

                    // I never want to fucking write that code again. Even my computer's had enough of this shit.
                    // Seriously.
                    // Windows is running at 5 FPS.
                    // WINDOWS.
                    // MICROSOFT. WINDOWS.
                    // At 5 FPS.
                    // Alright. Time to render shit.

                    // Top left corner.
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Top), brush.Color, texCoordTl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top), brush.Color, texCoordTopLeftCornerTr);
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Top + topEdgeHeight), brush.Color, texCoordTopLeftCornerBl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top), brush.Color, texCoordTopLeftCornerTr);
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Top + topEdgeHeight), brush.Color, texCoordTopLeftCornerBl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTl);

                    // Alright, computer? Can you handle the top-right corner?
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top), brush.Color, texCoordTopRightCornerTl);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Top), brush.Color, texCoordTr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTr);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Top), brush.Color, texCoordTr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTr);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Top + topEdgeHeight), brush.Color, texCoordTopRightCornerBr);

                    // Only 7 more of these shitty things to go.. I'll do the top edge now.
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top), brush.Color, texCoordTopLeftCornerTr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top), brush.Color, texCoordTopRightCornerTl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTl);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top), brush.Color, texCoordTopRightCornerTl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTl);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTr);

                    // Left edge.
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Top + topEdgeHeight), brush.Color, texCoordTopLeftCornerBl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTl);
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordBottomLeftCornerTl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTl);
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordBottomLeftCornerTl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBl);

                    // Right edge.
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTr);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Top + topEdgeHeight), brush.Color, texCoordTopRightCornerBr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBr);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Top + topEdgeHeight), brush.Color, texCoordTopRightCornerBr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBr);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordBottomRightCornerTr);

                    // Bottom left corner.
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordBottomLeftCornerTl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBl);
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Bottom), brush.Color, texCoordBl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBl);
                    renderItem.AddVertex(new Vector2(rect.Left, rect.Bottom), brush.Color, texCoordBl);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom), brush.Color, texCoordBottomLeftCornerBr);

                    // Bottom edge.
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBl);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBr);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom), brush.Color, texCoordBottomLeftCornerBr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBr);
                    renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom), brush.Color, texCoordBottomLeftCornerBr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Bottom), brush.Color, texCoordBottomRightCornerBl);

                    // Bottom-right corner.
                    renderItem.AddVertex(new Vector2(rect.Right - leftEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBr);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordBottomRightCornerTr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Bottom), brush.Color, texCoordBottomRightCornerBl);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordBottomRightCornerTr);
                    renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Bottom), brush.Color, texCoordBottomRightCornerBl);
                    renderItem.AddVertex(new Vector2(rect.Right, rect.Bottom), brush.Color, texCoordBr);

                    // OKAY.
                    // Can you, A10-7700K, handle ONE MORE?
                    // This one's an IF STATEMENT.
                    // Because box brushes have an inner filling.
                    if (brush.BrushType == BrushType.Box)
                    {
                        renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTl);
                        renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTr);
                        renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBl);
                        renderItem.AddVertex(new Vector2(rect.Right - rightEdgeWidth, rect.Top + topEdgeHeight), brush.Color, texCoordInnerTr);
                        renderItem.AddVertex(new Vector2(rect.Left + leftEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBl);
                        renderItem.AddVertex(new Vector2(rect.Right - leftEdgeWidth, rect.Bottom - bottomEdgeHeight), brush.Color, texCoordInnerBr);
                    }
                }
            }
        }

        
        /// <summary>
        /// Draws the given text to the screen.
        /// </summary>
        /// <param name="font">The font to draw the text with.</param>
        /// <param name="text">The text to draw on-screen.</param>
        /// <param name="position">The position to start drawing text at.</param>
        /// <param name="color">The color of each text character.</param>
        public void DrawString(DynamicSpriteFont font, string text, Vector2 position, Color color)
        {
            font.DrawString(this, text, position, color);
        }
        
        private Vector2 LinearMap(Vector2 value, RectangleF a, RectangleF b)
        {
            var nx = (value.X - a.Left) / a.Width;
            var ny = (value.Y - a.Top) / a.Height;

            return new Vector2((nx * b.Width) + b.Left, (ny * b.Height) + b.Top);
        }
        
        /* Credit where credit's due
         * =========================
         *
         * The following code is from https://github.com/Jjagg/OpenWheels,
         * and has been adapted to work within SpriteRocket2D.
         */
        
        /// <summary>
        /// Draws a straight line between two points.
        /// </summary>
        /// <param name="p1">The starting point of the line.</param>
        /// <param name="p2">The ending point of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="lineWidth">The thickness of the line.</param>
        public void DrawLine(Vector2 p1, Vector2 p2, Color color, float lineWidth = 1)
        {
            DrawLine(p1, p2, color, lineWidth, RectangleF.Unit);
        }

        private void CreateLine(Vector2 p1, Vector2 p2, Color color, float lineWidth, in RectangleF ur, out int v1, out int v2, out int v3, out int v4)
        {
            var renderItem = MakeRenderItem(null);
            
            var d = Vector2.Normalize(p2 - p1);
            var dt = new Vector2(-d.Y, d.X) * (lineWidth / 2f);

            v1 = renderItem.AddVertex(p1 + dt, color,ur.TopLeft);
            v2 = renderItem.AddVertex(p1 - dt, color, ur.TopRight);
            v3 = renderItem.AddVertex(p2 - dt, color, ur.BottomRight);
            v4 = renderItem.AddVertex(p2 + dt, color, ur.BottomLeft);
        }
        
        private void DrawLine(Vector2 p1, Vector2 p2, Color color, float lineWidth, in RectangleF uvRect)
        {
            var renderItem = MakeRenderItem(null);
            
            CreateLine(p1, p2, color, lineWidth, uvRect, out var v1, out var v2, out var v3, out var v4);

            renderItem.AddIndex(v1);
            renderItem.AddIndex(v2);
            renderItem.AddIndex(v3);
            renderItem.AddIndex(v2);
            renderItem.AddIndex(v3);
            renderItem.AddIndex(v4);
        }

        private const float RightStartAngle = 0;
        private const float RightEndAngle = (float) (2 * Math.PI);

        /// <summary>
        /// Draws lines between two or more points, as if the game was playing Connect the Dots.
        /// </summary>
        /// <param name="points">A sequence of two or more points to draw lines between.</param>
        /// <param name="color">The color of each line.</param>
        /// <param name="lineWidth">The thickness of each line.</param>
        public void DrawLineStrip(ReadOnlySpan<Vector2> points, Color color, float lineWidth = 1)
        {
            if (points.Length < 2)
                return;

            var p1 = points[0];
            var p2 = points[1];

            CreateLine(p1, p2, color, lineWidth, RectangleF.Unit, out var i1, out var i2, out var i3, out var i4);

            var renderItem = MakeRenderItem(null);

            var i3Prev = i3;
            var i4Prev = i4;
            
            renderItem.AddIndex(i1);
            renderItem.AddIndex(i2);
            renderItem.AddIndex(i3);
            renderItem.AddIndex(i2);
            renderItem.AddIndex(i3);
            renderItem.AddIndex(i4);

            p1 = p2;
            
            for (var i = 2; i < points.Length; i++)
            {
                p2 = points[i];

                CreateLine(p1, p2, color, lineWidth, RectangleF.Unit, out i1, out i2, out i3, out i4);

                renderItem.AddIndex(i1);
                renderItem.AddIndex(i2);
                renderItem.AddIndex(i3);
                renderItem.AddIndex(i2);
                renderItem.AddIndex(i3);
                renderItem.AddIndex(i4);

                renderItem.AddIndex(i3Prev);
                renderItem.AddIndex(i4Prev);
                renderItem.AddIndex(i2);

                i3Prev = i3;
                i4Prev = i4;
                p1 = p2;
            }

        }
        
        private void FillTriangleFan(Vector2 center, ReadOnlySpan<Vector2> vs, Color color)
        {
            var c = vs.Length;
            if (c < 2)
                throw new ArgumentException(@"Need at least 3 vertices for a triangle fan.", nameof(vs));

            var renderItem = MakeRenderItem(null);

            var centerIndex = renderItem.AddVertex(center, color, Vector2.Zero);
            var v1 = renderItem.AddVertex(vs[0], color, Vector2.Zero);
            for (var i = 1; i < c; i++)
            {
                var v2 = renderItem.AddVertex(vs[i], color, Vector2.Zero);
                renderItem.AddIndex(centerIndex);
                renderItem.AddIndex(v1);
                renderItem.AddIndex(v2);
                v1 = v2;
            }
        }
        
        /// <summary>
        /// Draws a filled circle.
        /// </summary>
        /// <param name="center">The center point of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The color to draw the circle with.</param>
        /// <param name="maxError">https://youtu.be/hQ3GW7lVBWY</param>
        public void FillCircle(Vector2 center, float radius, Color color, float maxError = .25f)
        {
            if (radius <= 1)
                return;
            
            FillCircleSegment(center, radius, RightStartAngle, RightEndAngle, color, maxError);
        }
        
        private static void CreateCircleSegment(Vector2 center, float radius, float step, float start, float end, ref Span<Vector2> result)
        {
            var i = 0;
            float theta;
            for (theta = start; theta < end; theta += step)
                result[i++] = new Vector2((float) (center.X + radius * Math.Cos(theta)), (float) (center.Y + radius * Math.Sin(theta)));

            if (Math.Abs(theta - end) > 0.00001f)
                result[i] = center + new Vector2((float) (radius * Math.Cos(end)), (float) (radius * Math.Sin(end)));
        }
        
        private void FillCircleSegment(Vector2 center, float radius, float start, float end, Color color, float maxError)
        {
            ComputeCircleSegments(radius, maxError, end - start, out var step, out var segments);

            Span<Vector2> points = stackalloc Vector2[segments + 1];
            CreateCircleSegment(center, radius, step, start, end, ref points);
            
            FillTriangleFan(center, points, color);
        }
        
        private void ComputeCircleSegments(float radius, float maxError, float range, out float step, out int segments)
        {
            var invErrRad = 1 - maxError / radius;
            step = (float) Math.Acos(2 * invErrRad * invErrRad - 1);
            segments = (int) (range / step + 0.999f);
        }
    }
}
