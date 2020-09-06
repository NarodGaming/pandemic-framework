using AlkalineThunder.Pandemic.Skinning.Json;

namespace AlkalineThunder.Pandemic.Skinning
{
    /// <summary>
    /// Represents layout data that's specified by a skin.
    /// </summary>
    public class SkinLayoutInfo
    {
        /// <summary>
        /// Gets the height in pixels of progress bars.
        /// </summary>
        public int ProgressBarHeight { get; private set; }

        internal static SkinLayoutInfo FromJsonData(JsonLayoutInfo info)
        {
            var skn = new SkinLayoutInfo();

            skn.ProgressBarHeight = info.ProgressBarHeight;

            return skn;
        }
    }
}