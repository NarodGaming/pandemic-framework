using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Skinning.Json
{
    [JsonObject]
    internal class JsonSkinMetadata
    {
        [JsonRequired]
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonRequired]
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonRequired]
        [JsonProperty("author")]
        public string Author { get; set; }
    }
}