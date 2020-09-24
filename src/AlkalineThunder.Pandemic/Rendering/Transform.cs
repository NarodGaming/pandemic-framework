using System;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Rendering
{
    public class Transform
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; } = Vector2.One;

        public Transform(Vector2 position, float rotation, Vector2 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
        
        public Transform() : this(Vector2.Zero, 0, Vector2.One) {}

        public Vector2 PerformTransform(Vector2 position)
        {
            var rotated = Vector2.Transform(position, Matrix.CreateRotationZ((MathF.PI / 180) * Rotation));
            var translated = Vector2.Transform(rotated, Matrix.CreateTranslation(Position.X, Position.Y, 0));
            var scaled = Vector2.Transform(translated, Matrix.CreateScale(Scale.X, Scale.Y, 1));

            return scaled;
        }
        
        public static Transform Default => new Transform();
    }
}