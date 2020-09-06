namespace AlkalineThunder.Pandemic.Settings
{
    /// <summary>
    /// Represents the user's settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Gets or sets a value indicating how the game should be presented to the user.
        /// </summary>
        public FullScreenMode FullScreenMode { get; set; } = FullScreenMode.Borderless;

        /// <summary>
        /// Gets or sets a value indicating the desired display resolution or window size of the game.
        /// </summary>
        public string ScreenResolution { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the game's user interface gets a dark skin.
        /// </summary>
        public bool EnableDarkMode { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the game's Discord Rich Presence feature is enabled.
        /// </summary>
        public bool EnableRichPresence { get; set; } = true;

        /// <summary>
        /// Gets or sets the volume of in-game sound effects.
        /// </summary>
        public float SoundEffectsVolume { get; set; } = 0.75f;

        /// <summary>
        /// Gets or sets the volume of in-game background music.
        /// </summary>
        public float MusicVolume { get; set; } = 0.75f;

        /// <summary>
        /// Gets or sets the GUI scale factor.
        /// </summary>
        public float GuiScale { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether the GUI renderer should display a highlight rectangle on the hovered
        /// user interface element (developer setting)
        /// </summary>
        public bool DevShowHoveredGuiElement { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the left and right mouse buttons are swapped by the input system.
        /// </summary>
        public bool SwapPrimaryMouseButton { get; set; }

        /// <summary>
        /// Gets or sets the name of the active skin.
        /// </summary>
        public string ActiveSkinName { get; set; } = "default";
        
        /// <summary>
        /// Gets or sets a value indicating whether vertical sync and fixed time stepping is enabled.
        /// </summary>
        public bool EnableVSync { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the blur effect can be used in certain UI elements.
        /// Disabling this setting may increase performance at the expense of the Hollywood
        /// hacker flare.
        /// </summary>
        public bool EnableBlurs { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the game allows transparency in the Terminal.
        /// </summary>
        public bool EnableTransparency { get; set; } = true;
    }
}
