using System.Collections.Generic;
using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Skinning.Json
{
    [JsonObject]
    internal class JsonSkinData
    {
        [JsonRequired]
        [JsonProperty("meta")]
        public JsonSkinMetadata Metadata { get; set; }
        
        [JsonProperty("colors")]
        public Dictionary<string, JsonColorScheme> ColorSchemes { get; set; }
        
        [JsonProperty("fonts")]
        public JsonFontList Fonts { get; set; }
        
        [JsonProperty("images")]
        public JsonSkinTextures Textures { get; set; } = new JsonSkinTextures();
        
        [JsonProperty("layout")]
        public JsonLayoutInfo LayoutInfo { get; set; } = new JsonLayoutInfo();
    }
}