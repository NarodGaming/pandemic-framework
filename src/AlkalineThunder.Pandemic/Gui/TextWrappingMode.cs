namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Represents a way in which the Text Renderer should wrap text.
    /// </summary>
    public enum TextWrappingMode
    {
        /// <summary>
        /// Render text as-is, with no wrapping. (Default SpriteRocket behaviour)
        /// </summary>
        None,

        /// <summary>
        /// Wrap text on each letter. (Fast)
        /// </summary>
        LetterWrap,

        /// <summary>
        /// Try to wrap text on a word when possible to avoid breaking words in half. (Recommended in most cases)
        /// </summary>
        WordWrap
    }
}
