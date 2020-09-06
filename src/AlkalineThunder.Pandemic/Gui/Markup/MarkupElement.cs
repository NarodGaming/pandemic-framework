using System;

namespace AlkalineThunder.Pandemic.Gui.Markup
{
    /// <summary>
    /// Marks a class as usable as an element inside GUI markup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MarkupElementAttribute : Attribute
    {
        /// <summary>
        /// Gets a value representing the name of the markup element.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="MarkupElementAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the markup element.</param>
        public MarkupElementAttribute(string name)
        {
            Name = name;
        }
}
}