using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui
{
    internal static class LayoutUtils
    {
        public static Rectangle CalculateBoundingBox(Rectangle parent, HorizontalAlignment hAlignment, VerticalAlignment vAlignment, Vector2 actualSize)
        {
            var boundingBox = Rectangle.Empty;

            switch (hAlignment)
            {
                case HorizontalAlignment.Stretch:
                    boundingBox.X = parent.Left;
                    boundingBox.Width = parent.Width;
                    break;
                case HorizontalAlignment.Left:
                    boundingBox.X = parent.Left;
                    boundingBox.Width = (int)actualSize.X;
                    break;
                case HorizontalAlignment.Center:
                    boundingBox.Width = (int)actualSize.X;
                    boundingBox.X = parent.Left + ((parent.Width - boundingBox.Width) / 2);
                    break;
                case HorizontalAlignment.Right:
                    boundingBox.Width = (int)actualSize.X;
                    boundingBox.X = parent.Right - boundingBox.Width;
                    break;
            }

            switch (vAlignment)
            {
                case VerticalAlignment.Stretch:
                    boundingBox.Y = parent.Top;
                    boundingBox.Height = parent.Height;
                    break;
                case VerticalAlignment.Top:
                    boundingBox.Y = parent.Top;
                    boundingBox.Height = (int)actualSize.Y;
                    break;
                case VerticalAlignment.Center:
                    boundingBox.Height = (int)actualSize.Y;
                    boundingBox.Y = parent.Top + ((parent.Height - boundingBox.Height) / 2);
                    break;
                case VerticalAlignment.Bottom:
                    boundingBox.Height = (int)actualSize.Y;
                    boundingBox.Y = parent.Bottom - boundingBox.Height;
                    break;
            }



            return boundingBox;
        }
    }
}
