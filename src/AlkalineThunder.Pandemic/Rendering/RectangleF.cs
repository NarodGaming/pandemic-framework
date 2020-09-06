using System;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Rendering
{
    /// <summary>
    /// Represents a floating-point rectangular area.
    /// </summary>
    public struct RectangleF
    {
        /// <summary>
        /// The horizontal position of the rectangle.
        /// </summary>
        public readonly float X;
        
        /// <summary>
        /// The vertical position of the rectangle.
        /// </summary>
        public readonly float Y;
        
        /// <summary>
        /// The horizontal size of the rectangle.
        /// </summary>
        public readonly float Width;
        
        /// <summary>
        /// The vertical size of the rectangle./
        /// </summary>
        public readonly float Height;

        /// <summary>
        /// Gets the left position of the rectangle.
        /// </summary>
        public float Left => X;
        
        /// <summary>
        /// Gets the top position of the rectangle.
        /// </summary>
        public float Top => Y;
        
        /// <summary>
        /// Gets the right position of the rectangle.
        /// </summary>
        public float Right => X + Width;
        
        /// <summary>
        /// Gets the bottom position of the rectangle.
        /// </summary>
        public float Bottom => Y + Height;

        /// <summary>
        /// Gets the top-left location of the rectangle.
        /// </summary>
        public Vector2 TopLeft => Location;
        
        /// <summary>
        /// Gets the top-right location of the rectangle.
        /// </summary>
        public Vector2 TopRight => Location + new Vector2(Width, 0);
        
        /// <summary>
        /// Gets the bottom left location of the rectangle.
        /// </summary>
        public Vector2 BottomLeft => Location + new Vector2(0, Height);
        
        /// <summary>
        /// Gets the bottom right location of the rectangle.
        /// </summary>
        public Vector2 BottomRight => Location + Size;
        
        /// <summary>
        /// Gets the location of the rectangle.
        /// </summary>
        public Vector2 Location => new Vector2(X, Y);
        
        /// <summary>
        /// Gets the size of the rectangle.
        /// </summary>
        public Vector2 Size => new Vector2(Width, Height);
        
        /// <summary>
        /// Gets the centre point of the rectangle.
        /// </summary>
        public Vector2 Center => Location + (Size / 2);
        
        /// <summary>
        /// Creates a new instance of the <see cref="RectangleF"/> structure.
        /// </summary>
        /// <param name="x">The rectangle's X coordinate</param>
        /// <param name="y">The rectangle's Y coordinate</param>
        /// <param name="w">The width of the rectangle.</param>
        /// <param name="h">The height of the rectangle.</param>
        public RectangleF(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RectangleF"/> structure from a position and
        /// size vector.
        /// </summary>
        /// <param name="position">The location of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        public RectangleF(Vector2 position, Vector2 size)
            : this(position.X, position.Y, size.X, size.Y)
        {}

        /// <summary>
        /// Creates a new instance of a <see cref="RectangleF"/> from a center point and half-extents.
        /// </summary>
        /// <param name="center">The center point of the rectangle.</param>
        /// <param name="extents">The rectangle's horizontal and vertical extrents.</param>
        /// <returns>The resulting rectangle value.</returns>
        public static RectangleF FromHalfExtents(Vector2 center, Vector2 extents)
        {
            var location = center - extents;
            var size = extents * 2;
            return new RectangleF(location, size);
        }

        /// <summary>
        /// Implicitly creates a new instance of the <see cref="RectangleF"/> structure from
        /// a normal <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The Rectangle structure to convert.</param>
        /// <returns>The exact same rectangle, but it's floating-point now. What the hell else did you expect? I have a headache.</returns>
        public static implicit operator RectangleF(Rectangle rect)
        {
            return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }
        
        /// <summary>
        /// Determines whether this rectangle intersects with the given rectangle.
        /// </summary>
        /// <param name="other">The rectangle representing the bounds to check.</param>
        /// <returns>A value indicating whether the two rectangles intersect.</returns>
        public bool IntersectsWith(RectangleF other)
        {
            if (other.Top > this.Bottom)
                return false;

            if (other.Left > this.Right)
                return false;

            if (this.Left > other.Right)
                return false;

            if (this.Top > other.Bottom)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Gets a rectangle representing an empty area.
        /// </summary>
        public static readonly RectangleF Empty = new RectangleF(Vector2.Zero, Vector2.Zero);
        
        /// <summary>
        /// Gets a rectangle representing a single unit.
        /// </summary>
        public static readonly RectangleF Unit = new RectangleF(Vector2.Zero, Vector2.One);
        
        /// <summary>
        /// Determines whether two rectangles are equal.
        /// </summary>
        /// <param name="a">The left-hand rectangle value.</param>
        /// <param name="b">The right-hand rectangle value.</param>
        /// <returns>A value indicating whether the two rectangles are, in fact, equal.</returns>
        public static bool operator ==(RectangleF a, RectangleF b)
        {
            return (Math.Abs(a.X - b.X) < 0.00001f 
                    && Math.Abs(a.Y - b.Y) < 0.00001f 
                    && Math.Abs(a.Width - b.Width) < 0.00001f 
                    && Math.Abs(a.Height - b.Height) < 0.00001f);
        }

        /// <summary>
        /// Determines whether two rectangles are unequal.
        /// </summary>
        /// <param name="a">The left-hand rectangle value.</param>
        /// <param name="b">The right-hand rectangle value.</param>
        /// <returns>A value indicating whether the two rectangles are, in fact, not equal.</returns>
        public static bool operator !=(RectangleF a, RectangleF b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is RectangleF rect && rect == this;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Width, Height);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"({X}, {Y}, {Width}, {Height})";
        }

        /// <summary>
        /// Computes and returns the area of intersection between two rectangles.
        /// </summary>
        /// <param name="a">The first intersecting rectangle.</param>
        /// <param name="b">The second intersecting rectangle.</param>
        /// <returns>A value representing the two rectangles' area of intersection.</returns>
        public static RectangleF Intersect(RectangleF a, RectangleF b)
        {
            if (a.IntersectsWith(b))
            {
                var top = Math.Max(a.Top, b.Top);
                var left = Math.Max(a.Left, b.Left);
                var bottom = Math.Min(a.Bottom, b.Bottom);
                var right = Math.Min(a.Right, b.Right);

                var width = right - left;
                var height = bottom - top;

                return new RectangleF(left, top, width, height);
            }
            else
            {
                return RectangleF.Empty;
            }
        }
    }
}