using AlkalineThunder.Pandemic.Skinning;
using SpriteFontPlus;

namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Represents the value of a dynamic font style property.
    /// </summary>
    public class FontStyle
    {
        private DynamicSpriteFont _customFont;
        private SkinFontStyle _skinFont;
        private bool _isCustom;

        private FontStyle(DynamicSpriteFont font)
        {
            _isCustom = true;
            _customFont = font;
        }

        private FontStyle(SkinFontStyle skinFont)
        {
            _isCustom = false;
            _skinFont = skinFont;
        }

        /// <summary>
        /// Gets the font object represented by this <see cref="FontStyle"/> instance.
        /// </summary>
        /// <param name="ctx">An object that provides access to the state of the game's user interface.</param>
        /// <returns>The SpriteFont instance of the font represented by the font style object.</returns>
        public DynamicSpriteFont GetFont(IGuiContext ctx)
        {
            return _isCustom ? _customFont : ctx.Skin.GetFont(_skinFont);
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="FontStyle"/> class whereby the font used is custom.
        /// </summary>
        /// <param name="font">The SpriteFont instance to use for the font.</param>
        /// <returns>A new <see cref="FontStyle"/> instance representing the given <paramref name="font"/>.</returns>
        public static implicit operator FontStyle(DynamicSpriteFont font)
        {
            return new FontStyle(font);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FontStyle"/> class whereby the font is defined by the current UI skin.
        /// </summary>
        /// <param name="style">A value representing the font style to retrieve from the skin.</param>
        /// <returns>A new <see cref="FontStyle"/> instance representing the skin font style.</returns>
        public static implicit operator FontStyle(SkinFontStyle style)
        {
            return new FontStyle(style);
        }
    }
}