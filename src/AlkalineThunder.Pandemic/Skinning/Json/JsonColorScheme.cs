using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Skinning.Json
{
    [JsonObject]
    internal class JsonColorScheme
    {
        [JsonProperty("background")] public string Background { get; set; }

        [JsonProperty("foreground")] public string Foreground { get; set; }

        [JsonProperty("primary")] public string Primary { get; set; }

        [JsonProperty("secondary")] public string Secondary { get; set; }

        [JsonProperty("info")] public string Info { get; set; }

        [JsonProperty("success")] public string Success { get; set; }

        [JsonProperty("error")] public string Error { get; set; }

        [JsonProperty("warning")] public string Warning { get; set; }

        [JsonProperty("feed.username")] public string FeedUsername { get; set; }

        [JsonProperty("feed.body")] public string FeedBodyText { get; set; }

        [JsonProperty("feed.avatar")] public string FeedAvatarBackground { get; set; }

        [JsonProperty("systembar.bg")] public string GatewaySystemBarBackground { get; set; }

        [JsonProperty("panel.bg")] public string GatewayPanelBackground { get; set; }

        [JsonProperty("terminal.bg")] public string GatewayTerminalBackground { get; set; }

        [JsonProperty("wallpaper")] public string GatewayWallpaper { get; set; }

        [JsonProperty("panel.border")] public string GatewayPanelBorder { get; set; }

        [JsonProperty("terminal.fg")] public string GatewayTerminalForeground { get; set; }

        [JsonProperty("panel.title.fg")] public string GatewayTitleText { get; set; }

        [JsonProperty("panel.title.bg")] public string GatewayTitle { get; set; }

        [JsonProperty("button.bg")] public string ButtonBackground { get; set; }

        [JsonProperty("button.fg")] public string ButtonForeground { get; set; }

        [JsonProperty("editor.gutter.bg")] public string EditorGutterBackground { get; set; }

        [JsonProperty("editor.gutter.fg")] public string EditorGutterForeground { get; set; }

        [JsonProperty("editor.highlight")] public string EditorHighlight { get; set; }

        [JsonProperty("editor.text")] public string EditorText { get; set; }

        [JsonProperty("branding.logo")] public string BrandingLogoTint { get; set; }

        public static readonly JsonColorScheme DefaultLight = new JsonColorScheme
        {
            Background = "#fdf6e3",
            Foreground = "#073642",
            Primary = "#2aa198",
            Secondary = "#268bd2",
            Success = "#859900",
            Warning = "#cb4b16",
            Error = "#dc322f",
            Info = "#6c71c4"
        };

        public static readonly JsonColorScheme DefaultDark = new JsonColorScheme
        {
            Background = "#002b36",
            Foreground = "#eee8d5",
            Primary = "#2aa198",
            Secondary = "#268bd2",
            Success = "#859900",
            Warning = "#cb4b16",
            Error = "#dc322f",
            Info = "#6c71c4"
        };
    }
}