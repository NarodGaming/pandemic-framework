using System.Linq.Expressions;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(string))]
    public class StringPropertyBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            return Expression.Constant(value);
        }
    }
}