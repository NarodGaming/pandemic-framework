using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Encapsulates the value of any color property of a <see cref="Control"/> and provides the
    /// ability to easily use dynamic colors from a <see cref="Skin"/>.
    /// </summary>
    public class ControlColor
    {
        private SkinColor _skinColor;
        private Color _customColor;
        private bool _isCustom;

        private ControlColor(SkinColor defined)
        {
            _skinColor = defined;
            _isCustom = false;
        }

        private ControlColor(Color custom)
        {
            _customColor = custom;
            _isCustom = true;
        }

        /// <summary>
        /// Retrieves the color represented by the <see cref="ControlColor"/>.
        /// </summary>
        /// <param name="ctx">An object which provides access to the state of the game's user interface.</param>
        /// <returns>The color value represented by this <see cref="ControlColor"/>.</returns>
        public Color GetColor(IGuiContext ctx)
        {
            if (_isCustom)
            {
                return _customColor;
            }
            else
            {
                return ctx.Skin.GetSkinColor(_skinColor);
            }
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="ControlColor"/> class which stores the
        /// color represented by the given HTML color string.
        /// </summary>
        /// <param name="hexCode">Any valid named HTML color or hex color code.</param>
        /// <returns>A <see cref="ControlColor"/> instance containing the given color.</returns>
        public static implicit operator ControlColor(string hexCode)
        {
            return new ControlColor(GameUtils.ParseHexColor(hexCode));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ControlColor"/> class which stores
        /// the specified color.
        /// </summary>
        /// <param name="customColor">The color to use for the control color.</param>
        /// <returns>A <see cref="ControlColor"/> object representing the given color.</returns>
        public static implicit operator ControlColor(Color customColor)
        {
            return new ControlColor(customColor);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ControlColor"/> class which represents
        /// the specified <see cref="SkinColor"/> value.
        /// </summary>
        /// <param name="definedColor">The <see cref="SkinColor"/> value representing the dynamic theme color to be used by the control color.</param>
        /// <returns>A new <see cref="ControlColor"/> instance that will dynamically return the given skin color.</returns>
        public static implicit operator ControlColor(SkinColor definedColor)
        {
            return new ControlColor(definedColor);
        }

        /// <summary>
        /// A <see cref="ControlColor"/> instance that uses the skin's default background color.
        /// </summary>
        public static readonly ControlColor Default = SkinColor.Default;
        
        /// <summary>
        /// <see cref="ControlColor"/> instance that uses the skin's primary accent color.
        /// </summary>
        public static readonly ControlColor Primary = SkinColor.Primary;

        /// <summary>
        /// <see cref="ControlColor"/> instance that uses the skin's secondary accent color.
        /// </summary>
        public static readonly ControlColor Secondary = SkinColor.Secondary;
        
        /// <summary>
        /// <see cref="ControlColor"/> instance that uses the skin's contextual information color.
        /// </summary>
        public static readonly ControlColor Info = SkinColor.Info;

        /// <summary>
        /// <see cref="ControlColor"/> instance that uses the skin's contextual success color.
        /// </summary>
        public static readonly ControlColor Success = SkinColor.Success;
        
        /// <summary>
        /// <see cref="ControlColor"/> instance that uses the skin's contextual error color.
        /// </summary>
        public static readonly ControlColor Error = SkinColor.Error;
        
        /// <summary>
        /// <see cref="ControlColor"/> instance that uses the skin's contextual warning color.
        /// </summary>
        public static readonly ControlColor Warning = SkinColor.Warning;
        
        /// <summary>
        /// <see cref="ControlColor"/> instance that uses the skin's default text color.
        /// </summary>
        public static readonly ControlColor Text = SkinColor.Text;
    }
}