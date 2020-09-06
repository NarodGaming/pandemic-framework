using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Rendering
{
    internal static class TextureCoords
    {
        public static readonly Vector2 TopLeft = Vector2.Zero;
        public static readonly Vector2 TopRight = new Vector2(1, 0);
        public static readonly Vector2 BottomLeft = new Vector2(0, 1);
        public static readonly Vector2 BottomRight = Vector2.One;
    }
}
