using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Skinning.Json
{
    [JsonObject]
    internal class JsonFontList
    {
        [JsonProperty("paragraph")] public JsonFont ParagraphFont { get; set; }
        [JsonProperty("code")] public JsonFont CodeFont { get; set; }
        [JsonProperty("h1")] public JsonFont Heading1Font { get; set; }
        [JsonProperty("h2")] public JsonFont Heading2Font { get; set; }
        [JsonProperty("h3")] public JsonFont Heading3Font { get; set; }
        [JsonProperty("overline")] public JsonFont OverlineFont { get; set; }
        [JsonProperty("feed.username")] public JsonFont FeedUsername { get; set; }
        [JsonProperty("feed.body")] public JsonFont FeedBody { get; private set; }
        
        [JsonProperty("gateway.panelTitle")]
        public JsonFont GatewayPanelTitle { get; set; }
        
        [JsonProperty("gateway.statusText")]
        public JsonFont StatusText { get; set; }
        
        [JsonProperty("input.text")]
        public JsonFont InputText { get; set; }
        
        [JsonProperty("list.item")]
        public JsonFont ListItem { get; set; }
    }
}