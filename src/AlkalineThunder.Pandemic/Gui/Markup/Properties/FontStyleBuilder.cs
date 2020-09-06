using System;
using System.Linq.Expressions;
using AlkalineThunder.Pandemic.Skinning;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(FontStyle))]
    public class FontStyleBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            if (Enum.TryParse(value, out SkinFontStyle style))
            {
                return Expression.Constant(style);
            }

            throw new FormatException($"'{value}': invalid font name");
        }
    }
}