using AlkalineThunder.Pandemic.Skinning.Json;

namespace AlkalineThunder.Pandemic.Skinning
{
    /// <summary>
    /// Represents the metadata of a loaded <see cref="Skin"/>.
    /// </summary>
    public class SkinMetadata
    {
        /// <summary>
        /// Gets the name of the skin.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Gets the name of the person who created the skin.
        /// </summary>
        public string Author { get; private set; }
        
        /// <summary>
        /// Gets the description text of the skin.
        /// </summary>
        public string Description { get; private set; }

        private SkinMetadata() {}
        
        internal static SkinMetadata FromJson(JsonSkinMetadata data)
        {
            var meta = new SkinMetadata();

            meta.Name = data.Name;
            meta.Author = data.Author;
            meta.Description = data.Description;
            
            return meta;
        }
    }
}