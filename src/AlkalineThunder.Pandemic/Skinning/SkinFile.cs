namespace AlkalineThunder.Pandemic.Skinning
{
    public struct SkinFile
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Author;
        public readonly string Description;

        public SkinFile(string id, string name, string author, string description)
        {
            Id = id;
            Name = name;
            Author = author;
            Description = description;
        }

        public static readonly SkinFile Invalid = new SkinFile("<invalid>", null, null, null);
    }
}