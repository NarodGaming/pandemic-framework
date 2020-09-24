using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Animation
{
    public interface IAnimation
    {
        bool IsFinished { get; }
        void Update(GameTime gameTime);
    }
}