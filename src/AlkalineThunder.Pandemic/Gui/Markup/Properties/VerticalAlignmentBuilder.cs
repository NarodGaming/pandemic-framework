using System;
using System.Linq.Expressions;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(VerticalAlignment))]
    public class VerticalAlignmentBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            switch (value)
            {
                case "top":
                    return Expression.Constant(VerticalAlignment.Top);
                case "center":
                    return Expression.Constant(VerticalAlignment.Center);
                case "bottom":
                    return Expression.Constant(VerticalAlignment.Bottom);
                case "stretch":
                    return Expression.Constant(VerticalAlignment.Stretch);
                default:
                    throw new FormatException($"'{value}': not a valid vertical alignment.");
            }
        }

    }
}