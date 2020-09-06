using System;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Rendering
{
    /// <summary>
    /// Represents a rectangular padding value.
    /// </summary>
    public struct Padding
    {
        /// <summary>
        /// The amount of padding on the left edge.
        /// </summary>
        public readonly float Left;
        
        /// <summary>
        /// The amount of padding on the top edge.
        /// </summary>
        public readonly float Top;
        
        /// <summary>
        /// The amount of padding on the right edge.
        /// </summary>
        public readonly float Right;
        
        /// <summary>
        /// The amount of padding on the bottom edge.
        /// </summary>
        public readonly float Bottom;

        /// <summary>
        /// Gets a value representing the total amount of horizontal padding.
        /// </summary>
        public float Horizontal => Left + Right;
        
        /// <summary>
        /// Gets a value representing the total amount of vertical padding.
        /// </summary>
        public float Vertical => Top + Bottom;

        /// <summary>
        /// Creates a new instance of the <see cref="Padding"/> structure.
        /// </summary>
        /// <param name="left">The amount of left padding.</param>
        /// <param name="top">The amount of top padding.</param>
        /// <param name="right">The amount of right padding.</param>
        /// <param name="bottom">The amount of bottom padding.</param>
        public Padding(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Padding"/> structure.
        /// </summary>
        /// <param name="all">The amount of padding on all sides.</param>
        public Padding(float all) : this(all, all, all, all) { }
        
        /// <summary>
        /// Creates a new instance of the <see cref="Padding"/> structure.
        /// </summary>
        /// <param name="x">The amount of horizontal padding.</param>
        /// <param name="y">The amount of vertical padding.</param>
        public Padding(float x, float y) : this(x, y, x, y) {}
        
        /// <summary>
        /// Implicitly creates a new instance of the <see cref="Padding"/> structure with a uniform padding value.
        /// </summary>
        /// <param name="all">The amount of padding on all sides.</param>
        /// <returns>The resulting uniform <see cref="Padding"/> value.</returns>
        public static implicit operator Padding(float all)
        {
            return new Padding(all);
        }

        /// <summary>
        /// Implicitly creates a new instance of the <see cref="Padding"/> structure using the given value
        /// as the horizontal and vertical padding.
        /// </summary>
        /// <param name="vector">The amount of vertical and horizontal padding to use.</param>
        /// <returns>The resulting <see cref="Padding"/> value.</returns>
        public static implicit operator Padding(Vector2 vector)
        {
            return new Padding(vector.X, vector.Y);
        }
        
        /// <summary>
        /// Determines whether two <see cref="Padding"/> values are equal.
        /// </summary>
        /// <param name="a">The left-hand padding value.</param>
        /// <param name="b">The right-hand padding value.</param>
        /// <returns>A value indicating whether the two values are equal.</returns>
        public static bool operator ==(Padding a, Padding b)
        {
            return (Math.Abs(a.Left - b.Left) < 0.00001f 
                    && Math.Abs(a.Right - b.Right) < 0.00001f 
                    && Math.Abs(a.Top - b.Top) < 0.00001f 
                    && Math.Abs(a.Bottom - b.Bottom) < 0.00001f);
        }

        /// <summary>
        /// Determines whether two <see cref="Padding"/> values are unequal.
        /// </summary>
        /// <param name="a">The left-hand padding value.</param>
        /// <param name="b">The right-hand padding value.</param>
        /// <returns>A value indicating whether the two values are unequal</returns>
        public static bool operator !=(Padding a, Padding b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is Padding p && p == this;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{{Left={Left}, Top={Top}, Right={Right}, Bottom={Bottom}}}";
        }

        /// <summary>
        /// Adds the given padding value to the specified <see cref="Vector2"/>, and returns the result.
        /// </summary>
        /// <param name="a">The left-hand 2D vector.</param>
        /// <param name="b">The padding value to add to the vector.</param>
        /// <returns>The resulting padded vector.</returns>
        public static Vector2 operator +(Vector2 a, Padding b)
        {
            return new Vector2(a.X + b.Horizontal, a.Y + b.Vertical);
        }
        
        /// <summary>
        /// Subtracts the given padding value from a vector and returns the result.
        /// </summary>
        /// <param name="a">The vector to subtract from.</param>
        /// <param name="b">The padding value to subtract.</param>
        /// <returns>The resulting subtracted vector.</returns>
        public static Vector2 operator -(Vector2 a, Padding b)
        {
            return new Vector2(a.X - b.Horizontal, a.Y - b.Vertical);
        }

        /// <summary>
        /// Deflates a rectangle with this padding and returns the result.
        /// </summary>
        /// <param name="rect">The rectangle to deflate.</param>
        /// <returns>The deflated rectangle value.</returns>
        public Rectangle Deflate(Rectangle rect)
        {
            return new Rectangle(
                rect.X + (int) this.Left,
                rect.Y + (int) this.Top,
                rect.Width - (int) this.Horizontal,
                rect.Height - (int) this.Vertical
            );
        }
    }
}
