namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Represents a named value that can be attached to other objects.
    /// </summary>
    /// <typeparam name="T">The type of value contained in the attached property.</typeparam>
    public class AttachedProperty<T> : IAttachedProperty
    {
        /// <summary>
        /// Gets or sets the name of the attached property.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the value stored in the attached property.
        /// </summary>
        public T Value { get; set; }

        object IAttachedProperty.Value
        {
            get => Value;
            set => this.Value = (T) value;
        }
    }
}