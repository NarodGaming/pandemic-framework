using System.Reflection;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Animation
{
    public sealed class FloatTransition : Animation<float>
    {
        public FloatTransition(object instance, float start, float end, float duration, MemberInfo member) : base(instance, start, end, duration, member)
        {
        }

        protected override float GetValue(float time, float duration)
        {
            return MathHelper.Lerp(StartValue, EndValue, MathHelper.Clamp(time / duration, 0, 1));
        }
    }
}