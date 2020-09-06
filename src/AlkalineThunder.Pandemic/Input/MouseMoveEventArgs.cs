using Microsoft.Xna.Framework.Input;

namespace AlkalineThunder.Pandemic.Input
{
    /// <summary>
    /// Represents the parameters of a mouse movement event.
    /// </summary>
    public sealed class MouseMoveEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Gets horizontal movement of the mouse from its previous position.
        /// </summary>
        public int DeltaX { get; }

        /// <summary>
        /// Gets vertical movement of the mouse from its previous position.
        /// </summary>
        public int DeltaY { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="MouseMoveEventArgs"/> class.
        /// </summary>
        /// <param name="state">The current state of the mouse when the event is fired.</param>
        /// <param name="deltaX">The horizontal movement of the mouse.</param>
        /// <param name="deltaY">The vertical movement of the mouse.</param>
        public MouseMoveEventArgs(MouseState state, int deltaX, int deltaY) : base(state)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }
    }
}
