namespace AlkalineThunder.Pandemic.Rendering
{
    /// <summary>
    /// Represents a type of <see cref="Brush"/>, that specifies how the brush is used by the renderer.
    /// </summary>
    public enum BrushType
    {
        /// <summary>
        /// Absolutely nothing is drawn to the screen.
        /// </summary>
        None,
        
        /// <summary>
        /// A solid color or image is drawn to the screen.
        /// </summary>
        Image,
        
        /// <summary>
        /// A solid or textured outline is drawn to the screen.
        /// </summary>
        Outline,
        
        /// <summary>
        /// A solid color or outlined image is drawn to the screen.
        /// </summary>
        Box
    }
}
