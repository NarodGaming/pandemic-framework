namespace AlkalineThunder.Pandemic.Settings
{
    /// <summary>
    /// Represents the way in which the game window is presented to the user.
    /// </summary>
    public enum FullScreenMode
    {
        /// <summary>
        /// The game is presented in a window that can be dragged around the screen and minimized.
        /// </summary>
        Windowed,

        /// <summary>
        /// The game runs in a borderless window that takes up the entirety of the display, but does not
        /// modify the hardware display mode in any way.
        /// </summary>
        Borderless,

        /// <summary>
        /// The game doesn't run in a window, and instead runs in full-screen - setting the hardware display mode to
        /// match the game's settings.
        /// </summary>
        FullScreen
    }
}
