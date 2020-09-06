using AlkalineThunder.Pandemic.Skinning.Json;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Skinning
{
    /// <summary>
    /// Represents the various colors used in a <see cref="Skin"/>.
    /// </summary>
    public class ColorScheme
    {
        /// <summary>
        /// Gets the default UI background color.
        /// </summary>
        public Color Background { get; private set; }
        
        /// <summary>
        /// Gets the default UI foreground (text) color.
        /// </summary>
        public Color Foreground { get; private set; }
        
        /// <summary>
        /// Gets the primary accent color.
        /// </summary>
        public Color Primary { get; private set; }
        
        /// <summary>
        /// Gets the secondary accent color.
        /// </summary>
        public Color Secondary { get; private set; }
        
        /// <summary>
        /// Gets the color used for information.
        /// </summary>
        public Color Info { get; private set; }
        
        /// <summary>
        /// Gets the color used for success.
        /// </summary>
        public Color Success { get; private set; }
        
        /// <summary>
        /// Gets the color used for errors.
        /// </summary>
        public Color Error { get; private set; }
        
        /// <summary>
        /// Gets the color used for warnings.
        /// </summary>
        public Color Warning { get; private set; }

        /// <summary>
        /// Gets the text color of a poster's username in the Feed.
        /// </summary>
        public Color FeedUsername { get; private set; }
        
        /// <summary>
        /// Gets the text color of a Feed post's body.
        /// </summary>
        public Color FeedBodyText { get; private set; }
        
        /// <summary>
        /// Gets the background color of a Feed poster's avatar.
        /// </summary>
        public Color FeedAvatar { get; private set; }
        
        /// <summary>
        /// Gets the background of the Terminal.
        /// </summary>
        public Color TerminalBackground { get; private set; }
        
        /// <summary>
        /// Gets the background color of the System Bar.
        /// </summary>
        public Color GatewaySystemBar { get; private set; }
        
        /// <summary>
        /// Gets the background color of Gateway OS panels.
        /// </summary>
        public Color GatewayPanel { get; private set; }
        
        /// <summary>
        /// Gets the wallpaper color for when a desktop wallpaper isn't specified.
        /// </summary>
        public Color Wallpaper { get; private set; }
        
        /// <summary>
        /// Gets the border color of Gateway panels.
        /// </summary>
        public Color GatewayPanelBorder { get; private set; }
        
        /// <summary>
        /// Gets the text color of the Terminal.
        /// </summary>
        public Color TerminalForeground { get; private set; }
        
        /// <summary>
        /// Gets the color of Gateway panels' title text.
        /// </summary>
        public Color PanelTitleText { get; private set; }
        
        /// <summary>
        /// Gets the background color of Gateway panel title bars.
        /// </summary>
        public Color PanelTitle { get; private set; }
        
        /// <summary>
        /// Gets the background color of the Editor's line number area.
        /// </summary>
        public Color EditorGutter { get; private set; }
        
        /// <summary>
        /// Gets the color of the Editor's line numbers.
        /// </summary>
        public Color EditorGutterText { get; private set; }
        
        /// <summary>
        /// Gets the background color of the active line in the Editor.
        /// </summary>
        public Color EditorHighlight { get; private set; }
        
        /// <summary>
        /// Gets the text color of the Editor.
        /// </summary>
        public Color EditorText { get; private set; }
        
        /// <summary>
        /// Gets the background color of buttons.
        /// </summary>
        public Color Button { get; private set; }
        
        /// <summary>
        /// Gets the text color of buttons where applicable.
        /// </summary>
        public Color ButtonText { get; private set; }
        
        /// <summary>
        /// Gets the color used to tint the Socially Distant menu logo.
        /// </summary>
        public Color LogoTint { get; private set; }
        
        internal static ColorScheme FromJsonColors(JsonColorScheme colors, JsonColorScheme baseColors)
        {
            var scheme = new ColorScheme();

            // Required colors.
            scheme.Background = GameUtils.ParseHexColor(colors.Background ?? baseColors.Background);
            scheme.Foreground = GameUtils.ParseHexColor(colors.Foreground ?? baseColors.Foreground);
            scheme.Primary = GameUtils.ParseHexColor(colors.Primary ?? baseColors.Primary);
            scheme.Secondary = GameUtils.ParseHexColor(colors.Secondary ?? baseColors.Secondary);
            scheme.Success = GameUtils.ParseHexColor(colors.Success ?? baseColors.Success);
            scheme.Info = GameUtils.ParseHexColor(colors.Info ?? baseColors.Info);
            scheme.Error = GameUtils.ParseHexColor(colors.Error ?? baseColors.Error);
            scheme.Warning = GameUtils.ParseHexColor(colors.Warning ?? baseColors.Warning);

            // Social feed colors.
            scheme.FeedUsername = GameUtils.ParseHexColor(colors.FeedUsername ?? colors.Foreground);
            scheme.FeedBodyText = GameUtils.ParseHexColor(colors.FeedBodyText ?? colors.Foreground);
            scheme.FeedAvatar = GameUtils.ParseHexColor(colors.FeedAvatarBackground ?? colors.Primary);

            // Gateway
            scheme.GatewaySystemBar = GameUtils.ParseHexColor(colors.GatewaySystemBarBackground ?? colors.Background);
            scheme.GatewayPanel = GameUtils.ParseHexColor(colors.GatewayPanelBackground ?? colors.Background);
            scheme.Wallpaper = GameUtils.ParseHexColor(colors.GatewayWallpaper ?? colors.Primary);
            scheme.GatewayPanelBorder = GameUtils.ParseHexColor(colors.GatewayPanelBorder ?? colors.Background);
            scheme.PanelTitleText = GameUtils.ParseHexColor(colors.GatewayTitleText ?? colors.Foreground);
            scheme.PanelTitle = (colors.GatewayTitle != null)
                ? GameUtils.ParseHexColor(colors.GatewayTitle)
                : scheme.GatewayPanelBorder; 
            
            // Terminal
            scheme.TerminalBackground = GameUtils.ParseHexColor(colors.GatewayTerminalBackground ?? colors.Background);
            scheme.TerminalForeground = GameUtils.ParseHexColor(colors.GatewayTerminalForeground ?? colors.Foreground);
            
            // logo
            scheme.LogoTint = GameUtils.ParseHexColor(colors.BrandingLogoTint ?? colors.Foreground);
            
            // editor
            scheme.EditorGutter = GameUtils.ParseHexColor(colors.EditorGutterBackground ?? "#00000000");
            scheme.EditorGutterText = GameUtils.ParseHexColor(colors.EditorGutterForeground ?? colors.Secondary);
            scheme.EditorHighlight = GameUtils.ParseHexColor(colors.EditorHighlight ?? "#00000000");
            scheme.EditorText = GameUtils.ParseHexColor(colors.EditorText ?? colors.Foreground);
            
            // Buttons
            scheme.Button = GameUtils.ParseHexColor(colors.ButtonBackground ?? colors.Secondary);
            scheme.ButtonText = GameUtils.ParseHexColor(colors.ButtonForeground ?? colors.Foreground);
            
            
            return scheme;
        }
    }
}