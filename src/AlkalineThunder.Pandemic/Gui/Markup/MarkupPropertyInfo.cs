using System.Reflection;

namespace AlkalineThunder.Pandemic.Gui.Markup
{
    internal class MarkupPropertyInfo
    {
        public MemberInfo MemberInfo { get; }
        public string Name { get; }

        public MarkupPropertyInfo(string name, MemberInfo member)
        {
            Name = name;
            MemberInfo = member;
        }
    }
}