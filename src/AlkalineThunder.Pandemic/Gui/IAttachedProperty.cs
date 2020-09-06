namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Represents a named value that can be attached to any object.
    /// </summary>
    public interface IAttachedProperty
    {
        /// <summary>
        /// Gets the name of the attached property.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets or sets the value of the property.
        /// </summary>
        object Value { get; set; }
    }
}