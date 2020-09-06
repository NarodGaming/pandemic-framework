using System.Linq.Expressions;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(bool))]
    public class BooleanBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            return Expression.Constant(bool.Parse(value));
        }
    }
}