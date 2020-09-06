using System;
using System.Linq.Expressions;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(ControlColor))]
    public class ControlColorBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            if (Enum.TryParse(value, out SkinColor color))
            {
                return Expression.Constant(color);
            }
            else if (GameUtils.TryParseHexColor(value, out Color hexColor))
            {
                return Expression.Constant(hexColor);
            }
            else
            {
                throw new FormatException($"'{value}': not a valid skin color or html color code.");
            }
        }
    }
}