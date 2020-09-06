using System.ComponentModel;
using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Skinning.Json
{
    [JsonObject]
    internal class JsonFont 
    {
        [DefaultValue("GenericFont")]
        [JsonRequired]
        [JsonProperty("path")]
        public string Path { get; set; }

        [DefaultValue(12)]
        [JsonProperty("size")] public float Size { get; set; } = 12;

        [JsonProperty("spacing")]
        public int Spacing { get; set; }
        
        [JsonProperty("lineSpacing")]
        public int LineSpacing { get; set; }

        public static readonly JsonFont Default = new JsonFont
        {
            Path = "GenericFont",
            Size = 12
        };

        public static readonly JsonFont DefaultParagraph = new JsonFont
        {
            Path = "Fonts/Ttf/Recursive-Regular.ttf",
            Size = 16
        };
        
        public static readonly JsonFont DefaultCode = new JsonFont
        {
            Path = "Fonts/Ttf/UbuntuMono-B.ttf",
            Size = 16
        };

        public static readonly JsonFont DefaultOverline = new JsonFont
        {
            Path = "Fonts/Ttf/Recursive-SemiBold.ttf",
            Size = 13,
            Spacing = 1
        };
        
        public static readonly JsonFont DefaultHeading1 = new JsonFont
        {
            Path = "Fonts/Ttf/UbuntuMono-B.ttf",
            Size = 24
        };

        public static readonly JsonFont DefaultHeading2 = new JsonFont
        {
            Path = "Fonts/Ttf/UbuntuMono-B.ttf",
            Size = 20
        };

        public static readonly JsonFont DefaultHeading3 = new JsonFont
        {
            Path = "Fonts/Ttf/UbuntuMono-B.ttf",
            Size = 18
        };






    }
}