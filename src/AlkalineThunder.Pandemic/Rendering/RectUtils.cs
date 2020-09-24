using System;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Rendering
{
    public static class RectUtils
    {
        public static Rectangle InflateWithTransform(this Rectangle rect, Transform transform)
        {
            // Get all four corners of the rect as Vector2's.
            var topLeft = rect.Location.ToVector2();
            var topRight = new Vector2(topLeft.X + rect.Width, topLeft.Y);
            var bottomLeft = new Vector2(topLeft.X, topLeft.Y + rect.Height);
            var bottomRight = new Vector2(topRight.X, bottomLeft.Y);
            
            // transform these points
            var ttl = transform.PerformTransform(topLeft);
            var ttr = transform.PerformTransform(topRight);
            var tbl = transform.PerformTransform(bottomLeft);
            var tbr = transform.PerformTransform(bottomRight);
            
            // Now we need to create a rectangle that includes these points.
            //
            // The way we want to do this is get the left- and right-most X coordinates
            // from those points, and the top- and bottom-most Y coordinates.
            //
            // With those, it's easy to create a rectangle using the left- and top-most
            // coordinates for the location, and the right-most and bottom-most minus the location
            // as the size.
            var leftMost = Math.Min(ttl.X, tbl.X);
            var topMost = Math.Min(ttl.Y, ttr.Y);
            var rightMost = Math.Max(ttr.X, tbr.X);
            var bottomMost = Math.Max(tbl.Y, tbr.Y);

            // Location is simple.
            var location = new Vector2(leftMost, topMost);
            
            // Size is a bit more annoying.
            var size = new Vector2(rightMost - leftMost, bottomMost - topMost);
            
            // And that's our inflated rectangle.
            return new Rectangle((int) location.X, (int) location.Y, (int) size.X, (int) size.Y);
        }
    }
}