using System.Reflection;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Animation
{
    public sealed class VectorTransition : Animation<Vector2>
    {
        public VectorTransition(object instance, Vector2 start, Vector2 end, float duration, MemberInfo member) : base(instance, start, end, duration, member)
        {
        }

        protected override Vector2 GetValue(float time, float duration)
        {
            return Vector2.Lerp(StartValue, EndValue, MathHelper.Clamp(time / duration, 0, 1));
        }
    }
}