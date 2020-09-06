using System;
using System.Linq;
using System.Linq.Expressions;
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(Padding))]
    public class PaddingBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            if (float.TryParse(value, out float uniform))
            {
                return Expression.Constant(uniform);
            }
            else
            {
                var numbers = value.Split(',').Select(x => float.Parse(x.Trim())).ToArray();

                switch (numbers.Count())
                {
                    case 2:
                        return Expression.Constant(new Vector2(numbers[0], numbers[1]));
                    case 4:
                        return Expression.Constant(new Padding(numbers[0], numbers[1], numbers[2], numbers[3]));
                }
            }

            throw new FormatException($"'{value}': invalid padding value");
        }
    }
}