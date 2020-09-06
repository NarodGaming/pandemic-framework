using System.Collections.Generic;
using System.IO;
using AlkalineThunder.Pandemic.Skinning.Json;
using SpriteFontPlus;

namespace AlkalineThunder.Pandemic.Skinning
{
    /// <summary>
    /// Represents a graphical user interface skin.
    /// </summary>
    public class Skin
    {
        /// <summary>
        /// Gets an object representing the skin's metadata info.
        /// </summary>
        public SkinMetadata Metadata { get; private set; }
        
        /// <summary>
        /// Gets the skin's light color theme.
        /// </summary>
        public ColorScheme LightColorScheme { get; private set; }
        
        /// <summary>
        /// Gets the skin's dark color theme.
        /// </summary>
        public ColorScheme DarkColorScheme { get; private set; }

        /// <summary>
        /// Gets the font used for list items.
        /// </summary>
        public DynamicSpriteFont ListItemFont { get; private set; }
        
        /// <summary>
        /// Gets the font used for the Gateway System Bar.
        /// </summary>
        public DynamicSpriteFont SystemBarFont { get; private set; }
        
        /// <summary>
        /// Gets the font used for Gateway panel titles.
        /// </summary>
        public DynamicSpriteFont PanelTitleFont { get; private set; }
        
        /// <summary>
        /// Gets the font used for input text.
        /// </summary>
        public DynamicSpriteFont InputFont { get; private set; }
        
        /// <summary>
        /// Gets the font used for normal paragraph text.
        /// </summary>
        public DynamicSpriteFont Paragraph { get; private set; }
        
        /// <summary>
        /// Gets the font used for top-level headings.
        /// </summary>
        public DynamicSpriteFont Heading1 { get; private set; }
        
        /// <summary>
        /// Gets the font used for second-level headings.
        /// </summary>
        public DynamicSpriteFont Heading2 { get; private set; }
        
        /// <summary>
        /// Gets the font used for third-level headings.
        /// </summary>
        public DynamicSpriteFont Heading3 { get; private set; }
        
        /// <summary>
        /// Gets the font used for code and the Terminal.
        /// </summary>
        public DynamicSpriteFont Code { get; private set; }
        
        /// <summary>
        /// Gets the font used for overline headings.
        /// </summary>
        public DynamicSpriteFont Overline { get; private set; }
        
        /// <summary>
        /// Gets the font used for Feed author usernames.
        /// </summary>
        public DynamicSpriteFont FeedUsernameFont { get; private set; }
        
        /// <summary>
        /// Gets the font used for Feed message text.
        /// </summary>
        public DynamicSpriteFont FeedBody { get; private set; }

        /// <summary>
        /// Gets an object containing the skin's loaded textures.
        /// </summary>
        public SkinTextureList Textures { get; private set; }

        /// <summary>
        /// Gets an object containing layout information for the skin.
        /// </summary>
        public SkinLayoutInfo LayoutInfo { get; private set; }
        
        private static string MapResourcePath(string root, string path)
        {
            if (path.StartsWith("~"))
            {
                if (!string.IsNullOrWhiteSpace(root) && Directory.Exists(root))
                {
                    path = path.Replace("~", "").Replace("/", Path.DirectorySeparatorChar.ToString());

                    while (path.StartsWith(Path.DirectorySeparatorChar))
                    {
                        path = path.Remove(0, 1);
                    }

                    return Path.Combine(root, path);
                }
                else
                {
                    throw new SkinLoadException($"Resource path ${path} not found.");
                }
            }

            return path;
        }

        private DynamicSpriteFont LoadFont(IGameContext ctx, JsonFont font, string root)
        {
            var path = font != null ? font.Path : JsonFont.Default.Path;
            var size = font != null ? font.Size : JsonFont.Default.Size;
            var spacing = font != null ? font.Spacing : 0;
            var lineSpacing = font != null ? font.LineSpacing : 0;
            
            DynamicSpriteFont loaded;

            if (path.StartsWith("~"))
            {
                var resPath = MapResourcePath(root, path);

                using (var stream = File.OpenRead(resPath))
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    loaded = DynamicSpriteFont.FromTtf(buffer, (int) size, 2048, 2048);
                }
            }
            else
            {
                try
                {
                    var contentPath = Path.Combine(ctx.GameLoop.Content.RootDirectory, path);
                    using (var s = File.OpenRead(contentPath))
                    {
                        var buffer = new byte[s.Length];
                        s.Read(buffer, 0, buffer.Length);

                        loaded = DynamicSpriteFont.FromTtf(buffer, (int) size, 2048, 2048);
                    }
                }
                catch
                {
                    throw new SkinLoadException(
                        $"The in-game resource at the path \"{path}\" is not a TTF font resource and cannot be used in this context.");
                }
                finally
                {
                    GameUtils.Log($"Loaded skin font resource: {path}");
                }
            }

            loaded.Spacing = spacing;
            loaded.LineSpacing = lineSpacing;
            
            return loaded;
        }

        private void LoadFontsFromJson(IGameContext ctx, JsonFontList fonts, string root)
        {
            GameUtils.Log("Loading fonts for skin.");
            this.Paragraph = this.LoadFont(ctx, fonts.ParagraphFont ?? JsonFont.DefaultParagraph, root);
            this.Code = this.LoadFont(ctx, fonts.CodeFont ?? JsonFont.DefaultCode, root);
            this.Heading1 = this.LoadFont(ctx, fonts.Heading1Font ?? JsonFont.DefaultHeading1, root);
            this.Heading2 = this.LoadFont(ctx, fonts.Heading2Font ?? JsonFont.DefaultHeading2, root);
            this.Heading3 = this.LoadFont(ctx, fonts.Heading3Font ?? JsonFont.DefaultHeading3, root);
            this.Overline = this.LoadFont(ctx, fonts.OverlineFont ?? JsonFont.DefaultOverline, root);

            // feed elements
            this.FeedUsernameFont =
                (fonts.FeedUsername != null) ? this.LoadFont(ctx, fonts.FeedUsername, root) : this.Overline;
            this.FeedBody =
                (fonts.FeedBody != null) ? this.LoadFont(ctx, fonts.FeedBody, root) : this.Paragraph;

            // Systembar font
            this.SystemBarFont = (fonts.StatusText != null) ? this.LoadFont(ctx, fonts.StatusText, root) : Overline;

            // Panel title
            this.PanelTitleFont = (fonts.GatewayPanelTitle != null)
                ? LoadFont(ctx, fonts.GatewayPanelTitle, root)
                : Overline;

            // Inputs
            this.InputFont = (fonts.InputText != null) ? LoadFont(ctx, fonts.InputText, root) : Paragraph;

            // List items
            this.ListItemFont = (fonts.ListItem != null) ? LoadFont(ctx, fonts.ListItem, root) : Paragraph;

            GameUtils.Log("All fonts successfully loaded.");
        }

        internal static Skin FromJsonSkin(IGameContext ctx, JsonSkinData data, string root = null)
        {
            GameUtils.Log($"About to load skin: {data.Metadata.Name} by {data.Metadata.Author}");

            var skin = new Skin();

            skin.Metadata = SkinMetadata.FromJson(data.Metadata);

            if (data.ColorSchemes == null)
                data.ColorSchemes = new Dictionary<string, JsonColorScheme>();
            
            if (!data.ColorSchemes.ContainsKey("light"))
                data.ColorSchemes.Add("light", JsonColorScheme.DefaultLight);
                
            if (!data.ColorSchemes.ContainsKey("dark"))
                data.ColorSchemes.Add("dark", JsonColorScheme.DefaultDark);
            
            skin.LightColorScheme = ColorScheme.FromJsonColors(data.ColorSchemes["light"], JsonColorScheme.DefaultLight);
            skin.DarkColorScheme = ColorScheme.FromJsonColors(data.ColorSchemes["dark"], JsonColorScheme.DefaultDark);

            skin.LoadFontsFromJson(ctx, data.Fonts ?? new JsonFontList(), root);

            skin.Textures = SkinTextureList.FromJsonTextures(ctx, data.Textures, root);
            skin.LayoutInfo = SkinLayoutInfo.FromJsonData(data.LayoutInfo);

            GameUtils.Log("Skin loaded.");

            return skin;
        }
    }
}