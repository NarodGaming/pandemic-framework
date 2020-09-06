using System;

namespace AlkalineThunder.Pandemic.Input
{
    /// <summary>
    /// REpresents a keyboard modifier key.
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>
        /// Represents absolutely nothing.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Represents any Ctrl key.
        /// </summary>
        Control = 1,
        
        /// <summary>
        /// Represents any Alt key.
        /// </summary>
        Alt = 2,
        
        /// <summary>
        /// Represents any Shift key. 
        /// </summary>
        Shift = 4
    }
}