using AlkalineThunder.Pandemic.Scenes;
using AlkalineThunder.Pandemic.Skinning;

namespace AlkalineThunder.Pandemic.Gui
{
    /// <summary>
    /// Provides access to the various components of the GUI system.
    /// </summary>
    public interface IGuiContext
    {
        /// <summary>
        /// Gets a reference to the engine's scene system.
        /// </summary>
        SceneSystem SceneSystem { get; }
        
        SkinSystem Skin { get; }
    }
}