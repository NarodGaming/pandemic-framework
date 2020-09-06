using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkalineThunder.Pandemic.Rendering
{
    /// <summary>
    /// Represents a solid or textured brush used to fill a polygon.
    /// </summary>
    public struct Brush
    {
        /// <summary>
        /// The texture to use for the brush.
        /// </summary>
        public readonly Texture2D Texture;
        
        /// <summary>
        /// The color of the brush.
        /// </summary>
        public readonly Color Color;
        
        /// <summary>
        /// Determines the sizes of the edges of the brush.
        /// </summary>
        public readonly Padding Margin;
        
        /// <summary>
        /// Specifies the type of the brush.
        /// </summary>
        public readonly BrushType BrushType;

        
        /// <summary>
        /// Creates a new instance of the <see cref="Brush"/> structure.
        /// </summary>
        /// <param name="texture">The texture to use for the brush.</param>
        /// <param name="color">The color of the brush.</param>
        /// <param name="brushType">The type of the brush.</param>
        /// <param name="margin">The size of the edges of the brush.</param>
        public Brush(Texture2D texture, Color color, BrushType brushType, Padding margin)
        {
            Texture = texture;
            Color = color;
            BrushType = brushType;
            Margin = margin;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Brush"/> structure.
        /// </summary>
        /// <param name="texture">The texture to use for the brush.</param>
        /// <param name="color">The color of the brush.</param>
        /// <param name="type">The type of the brush.</param>
        public Brush(Texture2D texture, Color color, BrushType type) : this(texture, color, type, 0) { }
        
        /// <summary>
        /// Creates a new instance of the <see cref="Brush"/> structure.
        /// </summary>
        /// <param name="texture">The texture of the brush.</param>
        /// <param name="color">The color of the brush.</param>
        public Brush(Texture2D texture, Color color) : this(texture, color, BrushType.Image) { }
        
        /// <summary>
        /// Creates a new instance of the <see cref="Brush"/> structure.
        /// </summary>
        /// <param name="texture">The texture of the brush.</param>
        public Brush(Texture2D texture) : this(texture, Color.White) { }
        
        /// <summary>
        /// Creates a new instance of the <see cref="Brush"/> structure.
        /// </summary>
        /// <param name="color">The color of the brush.</param>
        public Brush(Color color) : this(null, color) { }

        /// <summary>
        /// Represents a brush with no color or texture.
        /// </summary>
        public static readonly Brush None = new Brush(null, Color.White, BrushType.None);
        
        /// <summary>
        /// Represents a brush that draws a solid color or a colored image.
        /// </summary>
        public static readonly Brush Image = new Brush(null, Color.White, BrushType.Image);
        
        /// <summary>
        /// Represents a brush that draws a solid or textured outline.
        /// </summary>
        public static readonly Brush Outline = new Brush(null, Color.White, BrushType.Outline);
        
        /// <summary>
        /// Represents a brush that draws a solid color or an outlined box.
        /// </summary>
        public static readonly Brush Box = new Brush(null, Color.White, BrushType.Box);

        /// <summary>
        /// Determines whether two brushes are equal.
        /// </summary>
        /// <param name="a">The left-hand brush operand.</param>
        /// <param name="b">The right-hand brush operand.</param>
        /// <returns>A value indicating whether the two brushes are value-equal.</returns>
        public static bool operator ==(Brush a, Brush b)
        {
            return (a.Texture == b.Texture && a.Color == b.Color && a.BrushType == b.BrushType && a.Margin == b.Margin);
        }

        /// <summary>
        /// Determines whether two brushes are unequal.
        /// </summary>
        /// <param name="a">The left-hand brush operand.</param>
        /// <param name="b">The right-hand brush operand.</param>
        /// <returns>A value indicating whether the two brushes are value-unequal.</returns>
        public static bool operator !=(Brush a, Brush b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Texture, Color, BrushType, Margin);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Brush b && b == this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"(Color={Color}, Type={BrushType}, Margin={Margin})";
        }
    }
}
