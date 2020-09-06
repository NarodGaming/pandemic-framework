using System;

namespace AlkalineThunder.Pandemic.Gui.Markup
{
    internal class MarkupAttachedProperty
    {
        public Type PropertyType { get; }
        public string Name { get; }
        public string PropName => Name.Contains(".") ? Name.Substring(Name.LastIndexOf(".", StringComparison.Ordinal) + 1) : Name;
        
        public MarkupAttachedProperty(Type type, string name)
        {
            PropertyType = type;
            Name = name;
        }
    }
}