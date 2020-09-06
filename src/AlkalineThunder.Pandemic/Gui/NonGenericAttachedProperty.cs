namespace AlkalineThunder.Pandemic.Gui
{
    internal class NonGenericAttachedProperty : IAttachedProperty
    {
        public string Name { get; }
        public object Value { get; set; }

        public NonGenericAttachedProperty(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}