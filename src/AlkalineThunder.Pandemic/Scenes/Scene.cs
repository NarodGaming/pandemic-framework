using System;
using System.Threading.Tasks;
using AlkalineThunder.Pandemic.Gui;
using AlkalineThunder.Pandemic.Gui.Controls;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Scenes
{
    /// <summary>
    /// Provides the base functionality for a Pandemic Framework gameplay scene.
    /// </summary>
    public abstract class Scene : IGameContext, IGuiContext
    {
        private CanvasPanel _canvas = new CanvasPanel();

        /// <summary>
        /// Gets an instance of the scene's top-level GUI container element.
        /// </summary>
        public CanvasPanel Gui => _canvas;

        /// <inheritdoc />
        public T GetModule<T>() where T : EngineModule, new()
            => SceneSystem.GetModule<T>();

        /// <inheritdoc />
        public EngineModule GetModule(Type type)
            => SceneSystem.GetModule(type);

        public SkinSystem Skin
            => SceneSystem.Skin;
        
        /// <summary>
        /// Gets an instance of the Pandemic Framework's scene system module.
        /// </summary>
        public SceneSystem SceneSystem { get; private set; }

        SceneSystem IGuiContext.SceneSystem => this.SceneSystem;
        
        GameLoop IGameContext.GameLoop => SceneSystem.GameLoop;

        /// <summary>
        /// Gets an object containing all of the textures in the currently loaded GUI skin.
        /// </summary>
        protected SkinTextureList GuiTextures
            => Skin.Textures;

        /// <summary>
        /// Immediately closes the game.
        /// </summary>
        protected void ExitGame()
            => SceneSystem.GameLoop.Exit();

        /// <summary>
        /// Invokes an action on the game loop's main thread.
        /// </summary>
        /// <param name="action">The method to invoke on the game thread.</param>
        /// <returns>An awaitable task that completes when the game has had a chance to successfully invoke the given method.</returns>
        public async Task Invoke(Action action)
        {
            await SceneSystem.GameLoop.Invoke(action);
        }
        
        internal void Load(SceneSystem sceneSystem)
        {
            if (sceneSystem == null)
                throw new ArgumentNullException(nameof(sceneSystem));
            
            if (SceneSystem != null)
                throw new InvalidOperationException("Scene has already been loaded.");

            GameUtils.Log("Loading...");

            SceneSystem = sceneSystem;

            Gui.Scene = this;
            Gui.SceneSystem = SceneSystem;
            
            OnLoad();
        }

        internal void Unload()
        {
            if (SceneSystem == null)
                throw new InvalidOperationException("Scene has not been loaded yet.");

            GameUtils.Log("Unloading...");
            
            OnUnload();
            Gui.Clear();
            Gui.Scene = null;
            Gui.SceneSystem = null;
            SceneSystem = null;
        }

        internal void Update(GameTime gameTime)
        {
            OnUpdate(gameTime);

            Gui.Update(gameTime);
        }

        internal void Draw(GameTime gameTime, SpriteRocket2D renderer)
        {
            OnDraw(gameTime, renderer);

            Gui.Draw(gameTime, renderer);
        }
        
        /// <summary>
        /// Called every frame when it is time for the scene to render on-screen.
        /// </summary>
        /// <param name="gameTime">The amount of time since the last frame.</param>
        /// <param name="renderer">A <see cref="SpriteRocket2D"/> renderer that's ready and ripped for your pleasure.</param>
        protected virtual void OnDraw(GameTime gameTime, SpriteRocket2D renderer) {}

        /// <summary>
        /// Called every time the engine ticks, to allow the scene to update it's state.
        /// </summary>
        /// <param name="gameTime">The amount of time since the last update.</param>
        protected virtual void OnUpdate(GameTime gameTime) {}
        
        /// <summary>
        /// Called once when the scene first loads.
        /// </summary>
        protected virtual void OnLoad() {}
        
        /// <summary>
        /// Called once when the scene unloads.
        /// </summary>
        protected virtual void OnUnload() {}
    }
}