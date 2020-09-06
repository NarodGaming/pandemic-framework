using System;

namespace AlkalineThunder.Pandemic.Gui.Markup
{
    /// <summary>
    /// Exposes an instance property to GUI markup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MarkupPropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets a value representing the name of the property's markup attribute.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="MarkupPropertyAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the exposed markup property.</param>
        public MarkupPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}