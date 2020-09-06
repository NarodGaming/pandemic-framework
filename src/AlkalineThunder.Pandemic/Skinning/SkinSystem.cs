using System;
using System.Collections.Generic;
using System.IO;
using AlkalineThunder.Pandemic.CommandLine;
using AlkalineThunder.Pandemic.Settings;
using AlkalineThunder.Pandemic.Skinning.Json;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpriteFontPlus;

namespace AlkalineThunder.Pandemic.Skinning
{
    [RequiresModule(typeof(SettingsService))]
    public class SkinSystem : EngineModule
    {
        private Skin _skin;
        
        /// <summary>
        /// Gets the list of the textures stored in the currently loaded <see cref="Skin"/>.
        /// </summary>
        public SkinTextureList Textures => _skin.Textures;

        private string SkinsDirectory => Path.Combine(GameUtils.AppDataPath, "skins");
        
        /// <summary>
        /// Gets an object containing the layout information of the current skin.
        /// </summary>
        public SkinLayoutInfo LayoutInfo => _skin.LayoutInfo;

        /// <summary>
        /// Occurs when a skin has been successfully loaded.
        /// </summary>
        public event EventHandler SkinLoaded;

        /// <summary>
        /// Gets an object containing the current skin's metadata.
        /// </summary>
        public SkinMetadata ActiveSkin
            => _skin.Metadata;

        public SettingsService Settings
            => GetModule<SettingsService>();
        
        /// <summary>
        /// Loads an installed skin.
        /// </summary>
        /// <param name="name">The folder name of the installed skin to load.</param>
        /// <exception cref="SkinLoadException">The requested skin was not found or could not be loaded.</exception>
        [Exec("gui.loadSkin")]
        public void LoadSkin(string name)
        {
            if (name == "default")
            {
                LoadDefaultSkin();
            }
            else
            {
                var skinPath = Path.Combine(GameUtils.AppDataPath, "skins", name);
                if (File.Exists(Path.Combine(skinPath, "skin.json")))
                {
                    LoadSkinFromSourceDirectory(skinPath);
                }
                else
                {
                    throw new SkinLoadException($"{name}: Skin not found.");
                }
            }
        }

        public IEnumerable<SkinFile> GetAvailableSkins()
        {
            GameUtils.EnsureDirExists(SkinsDirectory);

            foreach (var dir in Directory.GetDirectories(SkinsDirectory))
            {
                var dirname = Path.GetFileName(dir);
                var jsonPath = Path.Combine(dir, "skin.json");
                if (File.Exists(jsonPath))
                {
                    var json = File.ReadAllText(jsonPath);
                    var skinData = JsonConvert.DeserializeObject<JsonSkinData>(json);

                    SkinFile file = SkinFile.Invalid;
                    
                    try
                    {
                        file = new SkinFile(dirname, skinData.Metadata.Name, skinData.Metadata.Author,
                            skinData.Metadata.Description);
                    }
                    catch
                    {
                        GameUtils.Log($"warning: {dirname} skin refused to load.");
                    }

                    yield return file;
                }
            }
        }

        /// <summary>
        /// Loads the default GUI skin. 
        /// </summary>
        [Exec("gui.resetSkin")]
        public void LoadDefaultSkin()
        {
            // Load the default skin file (TEMP)
            var skinJson = File.ReadAllText(Path.Combine(GameLoop.Content.RootDirectory, "skin.json"));
            var jsonSkinData = JsonConvert.DeserializeObject<JsonSkinData>(skinJson);
            _skin = Skin.FromJsonSkin(this, jsonSkinData);
            
            SkinLoaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Loads a skin directly from a source folder on the host file system.
        /// </summary>
        /// <param name="directory">The absolute path to the skin to load.</param>
        /// <exception cref="ShellException">The folder wasn't found, didn't have a skin.json file in it, or the skin failed to load due to a content error.</exception>
        [Exec("gui.loadSkinSrc")]
        public void LoadSkinFromSourceDirectory(string directory)
        {
            if (!Directory.Exists(directory))
                throw new ShellException($"Directory \"{directory}\" not found.");

            var skinMetadataPath = Path.Combine(directory, "skin.json");

            if (!File.Exists(skinMetadataPath))
                throw new ShellException(
                    "Cannot find skin.json in the root of the specified directory. This is not a skin.");

            var json = File.ReadAllText(skinMetadataPath);

            _skin = Skin.FromJsonSkin(this, JsonConvert.DeserializeObject<JsonSkinData>(json), directory);
            
            SkinLoaded?.Invoke(this, EventArgs.Empty);
        }

        public Color GetSkinColor(SkinColor color)
        {
            var scheme = Settings.EnableDarkTheme ? _skin.DarkColorScheme : _skin.LightColorScheme;
            
            switch (color)
            {
                case SkinColor.Default:
                    return scheme.Background;
                case SkinColor.Primary:
                    return scheme.Primary;
                case SkinColor.Secondary:
                    return scheme.Secondary;
                case SkinColor.Info:
                    return scheme.Info;
                case SkinColor.Error:
                    return scheme.Error;
                case SkinColor.Warning:
                    return scheme.Warning;
                case SkinColor.Text:
                    return scheme.Foreground;
                case SkinColor.FeedAvatar:
                    return scheme.FeedAvatar;
                case SkinColor.FeedUsername:
                    return scheme.FeedUsername;
                case SkinColor.FeedBody:
                    return scheme.FeedBodyText;
                case SkinColor.GatewayPanel:
                    return scheme.GatewayPanel;
                case SkinColor.GatewaySystemBar:
                    return scheme.GatewaySystemBar;
                case SkinColor.TerminalBackground:
                    return scheme.TerminalBackground;
                case SkinColor.Wallpaper:
                    return scheme.Wallpaper;
                case SkinColor.GatewayPanelBorder :
                    return scheme.GatewayPanelBorder;
                case SkinColor.PanelTitle:
                    return scheme.PanelTitle;
                case SkinColor.PanelTitleText:
                    return scheme.PanelTitleText;
                case SkinColor.TerminalForeground:
                    return scheme.TerminalForeground;
                case SkinColor.LogoTint:
                    return scheme.LogoTint;
                case SkinColor.Button:
                    return scheme.Button;
                case SkinColor.ButtonText:
                    return scheme.ButtonText;
                case SkinColor.EditorGutter:
                    return scheme.EditorGutter;
                case SkinColor.EditorGutterText:
                    return scheme.EditorGutterText;
                case SkinColor.EditorHighlight:
                    return scheme.EditorHighlight;
                case SkinColor.EditorText:
                    return scheme.EditorText;
                default:
                    return Color.Transparent;
            }
        }
        
        public DynamicSpriteFont GetFont(SkinFontStyle style)
        {
            switch (style)
            {
                case SkinFontStyle.Paragraph:
                    return _skin.Paragraph;
                case SkinFontStyle.Overline:
                    return _skin.Overline;
                case SkinFontStyle.Code:
                    return _skin.Code;
                case SkinFontStyle.Heading1:
                    return _skin.Heading1;
                case SkinFontStyle.Heading2:
                    return _skin.Heading2;
                case SkinFontStyle.Heading3:
                    return _skin.Heading3;
                case SkinFontStyle.FeedBody:
                    return _skin.FeedBody;
                case SkinFontStyle.FeedUsername:
                    return _skin.FeedUsernameFont;
                case SkinFontStyle.Systembar:
                    return _skin.SystemBarFont;
                case SkinFontStyle.Input:
                    return _skin.InputFont;
                case SkinFontStyle.ListItem:
                    return _skin.ListItemFont;
                case SkinFontStyle.PanelTitle:
                    return _skin.PanelTitleFont;
                default:
                    return _skin.Paragraph;
            }
        }

        protected override void OnLoadContent()
        {
            // This is where the featurebuild #2 comes in since we now load the skin from settings.
            if (Settings.ActiveSkinName == "default")
            {
                LoadDefaultSkin();
            }
            else
            {
                try
                {
                    var skindir = Path.Combine(GameUtils.AppDataPath, "skins");
                    GameUtils.EnsureDirExists(skindir);
                    var skinPath = Path.Combine(skindir, Settings.ActiveSkinName);
                    LoadSkinFromSourceDirectory(skinPath);
                }
                catch (Exception ex)
                {
                    GameUtils.Log(
                        $"SKIN LOAD WARNING: A custom skin could not be loaded because of the following exception. Falling back to tthe default skin.");
                    var exLines = ex.ToString().Split(Environment.NewLine);
                    
                    foreach (var exLine in exLines)
                        GameUtils.Log(exLine);

                    Settings.ActiveSkinName = "default";
                    LoadDefaultSkin();
                }
            }
        }
    }
}