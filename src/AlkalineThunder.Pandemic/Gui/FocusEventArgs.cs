using System;

namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Represents the parameters of an event generated when a user interface element gains
    /// or loses keyboard focus.
    /// </summary>
    public class FocusEventArgs : EventArgs
    {
        /// <summary>
        /// Gets an instance of the <see cref="Control"/> that lost keyboard focus.
        /// </summary>
        public Control LostFocus { get; }

        /// <summary>
        /// Gets an instance of the <see cref="Control"/> that gained keyboard focus.
        /// </summary>
        public Control GainedFocus { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="FocusEventArgs"/> class.
        /// </summary>
        /// <param name="lost">The control that lost keyboard focus.</param>
        /// <param name="gained">The control that gained keyboard focus.</param>
        public FocusEventArgs(Control lost, Control gained)
        {
            LostFocus = lost;
            GainedFocus = gained;
        }
    }
}