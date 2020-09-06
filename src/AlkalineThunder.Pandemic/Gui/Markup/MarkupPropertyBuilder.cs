using System.Linq.Expressions;

namespace AlkalineThunder.Pandemic.Gui.Markup
{
    /// <summary>
    /// Represents an object that's capable of parsing a markup attribute value
    /// into an abstract expression tree to be evaluated by the compiler.
    /// </summary>
    public abstract class MarkupPropertyBuilder
    {
        /// <summary>
        /// Parses the given value text into an expression tree.
        /// </summary>
        /// <param name="value">The text representing the value to parse.</param>
        /// <returns>An <see cref="Expression"/> representing the value.</returns>
        public abstract Expression BuildValueExpression(string value);
    }
}