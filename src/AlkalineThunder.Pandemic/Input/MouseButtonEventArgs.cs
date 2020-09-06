using Microsoft.Xna.Framework.Input;

namespace AlkalineThunder.Pandemic.Input
{
    /// <summary>
    /// Represents the parameters of any mouse event where a button's state has changed.
    /// </summary>
    public sealed class MouseButtonEventArgs : MouseEventArgs
    {
        /// <summary>
        /// Gets the mouse button that generated the event.
        /// </summary>
        public MouseButton Button { get; }

        /// <summary>
        /// Gets a value indicating the button's state.
        /// </summary>
        public ButtonState ButtonState { get; }

        /// <summary>
        /// Gets a value indicating whether the button is pressed down.
        /// </summary>
        public bool IsButtonDown => ButtonState == ButtonState.Pressed;

        /// <summary>
        /// Creates a new instance of the <see cref="MouseButtonEventArgs"/> class.
        /// </summary>
        /// <param name="state">The state of the mouse when the event is fired.</param>
        /// <param name="button">The mouse button that generated the event.</param>
        /// <param name="buttonState">The state of the mouse button.</param>
        public MouseButtonEventArgs(MouseState state, MouseButton button, ButtonState buttonState) : base(state)
        {
            Button = button;
            ButtonState = buttonState;
        }
    }
}
