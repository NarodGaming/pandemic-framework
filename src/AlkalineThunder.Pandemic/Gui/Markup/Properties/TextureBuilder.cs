using System.Linq.Expressions;
using Microsoft.Xna.Framework.Graphics;

namespace AlkalineThunder.Pandemic.Gui.Markup.Properties
{
    /// <inheritdoc />
    [MarkupType(typeof(Texture2D))]
    public class TextureBuilder : MarkupPropertyBuilder
    {
        /// <inheritdoc />
        public override Expression BuildValueExpression(string value)
        {
            return Expression.Call(typeof(GameLoop).GetMethod("LoadTexture") ?? throw new CompleteAndTotalFuckingIdiotDeveloperException("Someone got rid of the LoadTexture method or made it inaccessible to Reflection, so you can't use this markup feature anymore."), Expression.Constant(value));

        }
    }
}