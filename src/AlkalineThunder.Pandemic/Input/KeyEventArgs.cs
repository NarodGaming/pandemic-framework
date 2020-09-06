using System;
using Microsoft.Xna.Framework.Input;

namespace AlkalineThunder.Pandemic.Input
{
    /// <summary>
    /// Contains event data representing a keyboard event.
    /// </summary>
    public class KeyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the character that was pressed if any.
        /// </summary>
        public char Character { get; }
        
        /// <summary>
        /// Gets the key that was pressed or released.
        /// </summary>
        public Keys Key { get; }
        
        /// <summary>
        /// Gets the modifier keys that were pressed at the time of the event.
        /// </summary>
        public ModifierKeys Modifiers { get; }

        /// <summary>
        /// Gets a value indicating whether the Ctrl key was pressed.
        /// </summary>
        public bool Control => Modifiers.HasFlag(ModifierKeys.Control);
        
        /// <summary>
        /// Gets a value indicating whether the Alt key was pressed.
        /// </summary>
        public bool Alt => Modifiers.HasFlag(ModifierKeys.Alt);
        
        /// <summary>
        /// Gets a value indicating whether the Shift key was pressed.
        /// </summary>
        public bool Shift => Modifiers.HasFlag(ModifierKeys.Shift);
        
        /// <summary>
        /// Creates a new instance of the <see cref="KeyEventArgs"/> class.
        /// </summary>
        /// <param name="key">The key that generated the event.</param>
        /// <param name="modifiers">Any modifier keys that were pressed.</param>
        /// <param name="ch">The character, if any, represented by the keystroke.</param>
        public KeyEventArgs(Keys key, ModifierKeys modifiers, char ch = '\0')
        {
            Character = ch;
            Key = key;
            Modifiers = modifiers;
        }
    }
}