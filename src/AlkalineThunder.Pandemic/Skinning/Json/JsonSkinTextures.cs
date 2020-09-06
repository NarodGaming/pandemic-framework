using System.ComponentModel;
using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Skinning.Json
{
    [JsonObject]
    internal class JsonSkinTextures
    {
        [DefaultValue("Textures/CheckBox_Unchecked")]
        [JsonProperty("checkBox", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string CheckBoxUnchecked { get; set; } = "Textures/Checkbox_Unchecked";

        [DefaultValue("Textures/CheckBox_Checked")]
        [JsonProperty("checkBoxChecked", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string CheckBoxChecked { get; set; } = "Textures/Checkbox_Checked";
        
        [DefaultValue("Textures/CheckBox_Unknown")]
        [JsonProperty("checkBoxUnknown", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string CheckBoxUnknown { get; set; } = "Textures/Checkbox_Unknown";
        
        [JsonProperty("wallpaper")]
        public string Wallpaper { get; set; }
        
        [JsonProperty("systembar")]
        public string SystemBar { get; set; }
    }
}