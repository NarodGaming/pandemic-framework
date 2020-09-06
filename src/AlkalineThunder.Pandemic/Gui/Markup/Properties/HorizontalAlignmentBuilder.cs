using System;
using System.Linq.Expressions;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(HorizontalAlignment))]
    public class HorizontalAlignmentBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            switch (value)
            {
                case "left":
                    return Expression.Constant(HorizontalAlignment.Left);
                case "center":
                    return Expression.Constant(HorizontalAlignment.Center);
                case "right":
                    return Expression.Constant(HorizontalAlignment.Right);
                case "stretch":
                    return Expression.Constant(HorizontalAlignment.Stretch);
                default:
                    throw new FormatException($"'{value}': not a valid horizontal alignment.");
            }
        }
    }
}