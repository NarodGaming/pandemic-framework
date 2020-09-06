using System.Linq.Expressions;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(float))]
    public class SingleBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            return Expression.Constant(float.Parse(value));
        }
    }
}