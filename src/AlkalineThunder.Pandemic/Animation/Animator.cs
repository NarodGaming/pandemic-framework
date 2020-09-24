using System;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AlkalineThunder.Pandemic.Animation
{
    public sealed class Animator
    {
        private List<IAnimation> _animations = new List<IAnimation>();
        private List<int> _animationsToRemove = new List<int>();
        
        public void Update(GameTime gameTime)
        {
            for (var i = 0; i < _animations.Count; i++)
            {
                var anim = _animations[i];
                anim.Update(gameTime);
                if (anim.IsFinished)
                    _animationsToRemove.Add(i);
            }

            while (_animationsToRemove.Count > 0)
            {
                var last = _animationsToRemove.Last();
                _animationsToRemove.Remove(last);
                _animations.RemoveAt(last);
            }
        }

        private MemberInfo GetMemberInfo<T, U>(Expression<Func<T, U>> memberExp)
        {
            var body = memberExp.Body as MemberExpression ??
                       throw new InvalidOperationException("Expression is not a member.");
            
            return body.Member;
        }
        
        public void SmoothTransition<T>(T obj, Expression<Func<T, float>> member, float start, float end,
            float duration)
        {
            var memberInfo = GetMemberInfo(member);

            var anim = new FloatTransition(obj, start, end, duration, memberInfo);

            _animations.Add(anim);
        }
    }
    
    
}