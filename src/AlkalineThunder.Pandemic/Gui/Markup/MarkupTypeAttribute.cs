using System;

namespace AlkalineThunder.Pandemic.Gui.Markup
{
    /// <summary>
    /// If used on a derived class of type <see cref="MarkupPropertyBuilder"/>, this attribute will
    /// mark the property builder as being able to parse values of the given type.  If used on a static
    /// string field, this attribute will expose an Attached Property to the markup system with the string
    /// field's value being the name of the Attached Property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
    public class MarkupTypeAttribute : Attribute
    {
        /// <summary>
        /// Gets the type information represented by this attribute.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="MarkupTypeAttribute"/> class.
        /// </summary>
        /// <param name="type">The value or object type to expose to the markup system.</param>
        public MarkupTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}