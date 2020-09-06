using Microsoft.Xna.Framework.Input;
using System;

namespace AlkalineThunder.Pandemic.Input
{
    /// <summary>
    /// Represents the base parameters of any mouse event.
    /// </summary>
    public abstract class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current state of the mouse as of the event firing.
        /// </summary>
        public MouseState MouseState { get; }

        /// <summary>
        /// Gets the X position of the mouse when the event was fired.
        /// </summary>
        public int X => MouseState.X;

        /// <summary>
        /// Gets the Y position of the mouse when the event was fired.
        /// </summary>
        public int Y => MouseState.Y;

        /// <summary>
        /// Creates a new instance of the <see cref="MouseEventArgs"/> class.
        /// </summary>
        /// <param name="state">The current state of the mouse when the event is fired.</param>
        public MouseEventArgs(MouseState state)
        {
            MouseState = state;
        }
    }
}
