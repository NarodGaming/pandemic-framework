namespace AlkalineThunder.Pandemic.Gui.Controls
{
    /// <summary>
    /// Represents the state of a <see cref="CheckBox"/> control.
    /// </summary>
    public enum CheckState
    {
        /// <summary>
        /// The control is unchecked.
        /// </summary>
        Unchecked,
        
        /// <summary>
        /// The state of the control is unknown; it is neither checked nor unchecked.
        /// </summary>
        Checked,
        
        /// <summary>
        /// The control is currently checked.
        /// </summary>
        Unknown
    }
}