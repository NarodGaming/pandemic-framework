using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Skinning.Json
{
    [JsonObject]
    internal class JsonLayoutInfo
    {
        [JsonProperty("progressBarHeight")]
        public int ProgressBarHeight { get; set; } = 4;
    }
}