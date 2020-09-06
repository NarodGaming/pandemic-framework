using System;
using AlkalineThunder.Pandemic.Rendering;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Provides the base functionality for all core systems in Socially Distant's game engine.
    /// </summary>
    public abstract class EngineModule : IGameContext
    {
        private bool _initialized;
        private bool _loaded;
        
        GameLoop IGameContext.GameLoop => this.GameLoop;
        
        /// <summary>
        /// Gets a reference to the <see cref="GameLoop"/> class that is actively responsible
        /// for managing this service.
        /// </summary>
        public GameLoop GameLoop { get; private set; }

        /// <summary>
        /// Gets an instance of the specified engine module.
        /// </summary>
        /// <typeparam name="T">A type of <see cref="EngineModule" />.</typeparam>
        /// <returns>The instance of the engine module, or null if the module has not yet been loaded.</returns>
        public T GetModule<T>() where T : EngineModule, new()
            => GameLoop.GetModule<T>();

        /// <summary>
        /// Gets an instance of an engine module of the specified type.
        /// </summary>
        /// <param name="type">The type of engine module to find.</param>
        /// <returns>An instance of the engine module, or null if it has not yet been loaded.</returns>
        public EngineModule GetModule(Type type)
            => GameLoop.GetModule(type);
        
        /// <summary>
        /// Registers this module with the specified <paramref name="gameLoop" />.
        /// </summary>
        /// <param name="gameLoop">The <see cref="GameLoop" /> to attach this module to.</param>
        /// <exception cref="ModuleException">This module is already attached to a game loop, or another instance of this module type is attached to it.</exception>
        public void Register(GameLoop gameLoop)
        {
            if (this.GameLoop != null)
                throw new ModuleException("Module has already been registered with the game.");

            this.GameLoop = gameLoop ?? throw new ArgumentNullException(nameof(gameLoop));
        }

        /// <summary>
        /// Initializes this engine module.
        /// </summary>
        public void Initialize()
        {
            if (!_initialized)
            {
                _initialized = true;
                OnInitialize();
            }
        }

        /// <summary>
        /// Loads any content that this engine module needs.
        /// </summary>
        public void LoadContent()
        {
            if (!_loaded)
            {
                _loaded = true;
                OnLoadContent();
            }
        }

        /// <summary>
        /// Updates this engine module.
        /// </summary>
        /// <param name="gameTime">The amount of time since the last frame.</param>
        public void Update(GameTime gameTime)
        {
            OnUpdate(gameTime);
        }

        /// <summary>
        /// Fully unloads this module.
        /// </summary>
        public void Unload()
        {
            if (_initialized && _loaded)
            {
                OnUnload();
                _initialized = false;
                _loaded = false;
            }
        }

        /// <summary>
        /// Allows this module to draw to the screen.
        /// </summary>
        /// <param name="gameTime">The amount of time since the last frame.</param>
        /// <param name="renderer">An instance of the engine's renderer.</param>
        public void Draw(GameTime gameTime, SpriteRocket2D renderer)
        {
            OnDraw(gameTime, renderer);
        }
        
        /// <summary>
        /// Occurs when it is time for the engine to render.
        /// </summary>
        /// <param name="gameTime">The amount of time since the last frame.</param>
        /// <param name="renderer">An instance of the engine's renderer.</param>
        protected virtual void OnDraw(GameTime gameTime, SpriteRocket2D renderer) {}
        
        /// <summary>
        /// Called when the module is unloading.
        /// </summary>
        protected virtual void OnUnload() {}
        
        /// <summary>
        /// Called when the engine updates.
        /// </summary>
        /// <param name="gameTime">The amount of time since the last frame.</param>
        protected virtual void OnUpdate(GameTime gameTime) {}
        
        /// <summary>
        /// Called when the module is able to load content.
        /// </summary>
        protected virtual void OnLoadContent() {}
        
        /// <summary>
        /// Called when the module initializes.
        /// </summary>
        protected virtual void OnInitialize() {}
    }
}
