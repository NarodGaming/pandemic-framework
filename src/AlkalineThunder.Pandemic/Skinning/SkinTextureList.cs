using System;
using System.IO;
using AlkalineThunder.Pandemic.Skinning.Json;
using Microsoft.Xna.Framework.Graphics;

namespace AlkalineThunder.Pandemic.Skinning
{
    /// <summary>
    /// Contains all of the textures loaded by a skin.
    /// </summary>
    public class SkinTextureList
    {
        /// <summary>
        /// Gets the unchecked image for check boxes.
        /// </summary>
        public Texture2D CheckBoxUnchecked { get; private set; }
        
        /// <summary>
        /// Gets the "unknown" image of check boxes.
        /// </summary>
        public Texture2D CheckBoxUnknown { get; private set; }
        
        /// <summary>
        /// Gets the checked image of check boxes.
        /// </summary>
        public Texture2D CheckBoxChecked { get; private set; }
        
        /// <summary>
        /// Gets the wallpaper image.
        /// </summary>
        public Texture2D Wallpaper { get; private set; }
        
        /// <summary>
        /// Gets the background image for the System Bar.
        /// </summary>
        public Texture2D SystemBar { get; private set; }
        
        private static Texture2D LoadTexture(IGameContext ctx, string path, string root)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            
            var loadFromSkin = false;
            if (path.StartsWith("~") && !string.IsNullOrWhiteSpace(root))
            {
                loadFromSkin = true;
                path = path.Remove(0, 1).Replace('/', Path.DirectorySeparatorChar);
                while (path.StartsWith(Path.DirectorySeparatorChar))
                {
                    path = path.Remove(0, 1);
                }

                path = Path.Combine(root, path);
            }

            Texture2D texture;

            try
            {
                if (loadFromSkin)
                {
                    texture = Texture2D.FromFile(ctx.GameLoop.GraphicsDevice, path);
                }
                else
                {
                    texture = ctx.GameLoop.Content.Load<Texture2D>(path);
                }
            }
            catch (Exception ex)
            {
                throw new SkinLoadException($"The image resource '{path}' could not be loaded.", ex);
            }
            finally
            {
                GameUtils.Log($"Loaded texture: {path}");
            }
            
            return texture;
        }
        
        internal static SkinTextureList FromJsonTextures(IGameContext ctx, JsonSkinTextures textures, string root = null)
        {
            GameUtils.Log("Loading skin textures...");
            var textureList = new SkinTextureList();

            textureList.CheckBoxUnchecked = LoadTexture(ctx, textures.CheckBoxUnchecked, root);
            textureList.CheckBoxChecked = LoadTexture(ctx, textures.CheckBoxChecked, root);
            textureList.CheckBoxUnknown = LoadTexture(ctx, textures.CheckBoxUnknown, root);
            textureList.Wallpaper = LoadTexture(ctx, textures.Wallpaper, root);
            textureList.SystemBar = LoadTexture(ctx, textures.SystemBar, root);
            return textureList;
        }
    }
}