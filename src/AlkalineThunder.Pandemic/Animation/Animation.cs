using System;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Animation
{
    public abstract class Animation<T> : IAnimation
    {
        private T _start;
        private T _end;
        private float _duration;
        private float _time;
        private MemberInfo _memberInfo;
        private object _instance;

        public bool IsFinished => _time >= _duration;

        public T StartValue => _start;
        public T EndValue => _end;
        
        private void CheckMemberType(Type expected, MemberInfo member)
        {
            if (member is PropertyInfo prop)
            {
                if (prop.PropertyType != expected)
                    throw new InvalidOperationException("Animation type and member type doesn't match.");
            }
            else if (member is FieldInfo info)
            {
                if (info.FieldType != expected)
                    throw new InvalidOperationException("Animation type and member type doesn't match.");
            }
        }
        
        public Animation(object instance, T start, T end, float duration, MemberInfo member)
        {
            CheckMemberType(typeof(T), member);
            
            _instance = instance;
            _start = start;
            _end = end;
            _duration = duration;
            _memberInfo = member;
        }

        public void Update(GameTime gameTime)
        {
            _time += (float) gameTime.ElapsedGameTime.TotalSeconds;

            var value = GetValue(_time, _duration);

            if (_memberInfo is PropertyInfo prop)
            {
                prop.SetValue(_instance, value);
            }
            else if (_memberInfo is FieldInfo info)
            {
                info.SetValue(_instance, value);
            }
        }

        protected abstract T GetValue(float time, float duration);
    }
}