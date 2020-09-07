using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AlkalineThunder.Pandemic.CommandLine;
using AlkalineThunder.Pandemic.Debugging;
using AlkalineThunder.Pandemic.Rendering;
using AlkalineThunder.Pandemic.Settings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Provides access to the core functionality of the Socially Distant game engine.
    /// </summary>
    /// <remarks>
    /// The name "CovidGame" refers to exactly what one would think, the novel coronavirus -
    /// otherwise known as SARS-CoV-2, "the pandemic," or the entire reason this game exists
    /// in the first place.
    /// 
    /// This class implements the MonoGame game loop.  It exists
    /// for the entire lifecycle of the game.  Features such as
    /// the Scene Stack are implemented and accessible here.  This class can load, push, pop and unload scenes, take screenshots, and can exit the game.
    /// </remarks>
    public sealed class GameLoop : Game
    {
        private bool _reachedInit;
        private bool _reachedLoadContent;
        private GraphicsDeviceManager _graphics;
        private ConcurrentQueue<Task> _taskQueue = new ConcurrentQueue<Task>();
        private SpriteRocket2D _renderer;
        private List<EngineModule> _activeModules = new List<EngineModule>();
        private RenderTarget2D _renderTarget;
        
        /// <summary>
        /// Gets an instance of the engine's developer console.
        /// </summary>
        public DevConsole DevConsole => GetModule<DevConsole>();
        
        /// <summary>
        /// Gets the width of the game's viewport, in local (scaled) units.
        /// </summary>
        public float LocalWidth => GameUtils.BaseResolution != null
            ? GameUtils.BaseResolution.GetValueOrDefault().X
            : GraphicsDevice.PresentationParameters.BackBufferWidth;
        
        /// <summary>
        /// Gets the height of the game's viewport, in local (scaled) units.
        /// </summary>
        public float LocalHeight => GameUtils.BaseResolution != null
            ? GameUtils.BaseResolution.GetValueOrDefault().Y
            : GraphicsDevice.PresentationParameters.BackBufferHeight;

        /// <summary>
        /// Gets the current <see cref="GameLoop"/> instance.
        /// </summary>
        public static GameLoop CurrentGame { get; private set; }
        
        /// <summary>
        /// Gets an instance of the Gaussian blur effect built into the engine.
        /// </summary>
        public Effect Blur { get; private set; }
        
        /// <summary>
        /// Creates a new instance of the <see cref="GameLoop" /> class.
        /// </summary>
        public GameLoop()
        {
            if (CurrentGame != null)
                throw new InvalidOperationException("A game is already running in this process.");
            
            CurrentGame = this;
            
            _graphics = new GraphicsDeviceManager(this);
            
            Content.RootDirectory = !string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.BaseDirectory) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content") : "Content";

            IsMouseVisible = true;
            IsFixedTimeStep = false;
        }

        /// <summary>
        /// Determines whether a module is loaded.
        /// </summary>
        /// <typeparam name="T">The type of module to look for.</typeparam>
        /// <returns>A value indicating whether the module is loaded.</returns>
        public bool IsModuleActive<T>() where T : EngineModule, new()
            => IsModuleActive(typeof(T));

        /// <summary>
        /// Determines whether a module is loaded.
        /// </summary>
        /// <param name="type">The type of module to look for.</param>
        /// <returns>A value indicating whether the module is loaded.</returns>
        public bool IsModuleActive(Type type)
            => _activeModules.Any(x => x.GetType() == type);

        
        private void RegisterModule(EngineModule module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            if (IsModuleActive(module.GetType()))
                return;

            _activeModules.Add(module);

            if (_reachedInit)
                module.Initialize();

            if (_reachedLoadContent)
                module.LoadContent();

            if (IsModuleActive<DevConsole>())
            {
                GetModule<DevConsole>().RegisterCommandsInternal(module);
            }
        }
        
        /// <summary>
        /// Loads a texture from the specified path.  Used by the GUI markup system as a hack.
        /// </summary>
        /// <param name="path">The path of the texture to load.</param>
        /// <returns>An instance of the loaded texture.</returns>
        public static Texture2D LoadTexture(string path)
        {
            return CurrentGame.Content.Load<Texture2D>(path);
        }
        

        /// <summary>
        /// Gets an instance of an <see cref="EngineModule"/> of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="EngineModule"/> to find.</typeparam>
        /// <returns>The currently active instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="ModuleException">No modules of the given type were loaded.</exception>
        public T GetModule<T>() where T : EngineModule, new()
        {
            if (IsModuleActive<T>())
                return _activeModules.FirstOrDefault(x => x is T) as T;

            throw new ModuleException($"Module {typeof(T).FullName} is not loaded.");
        }

        /// <summary>
        /// Gets an instance of a loaded engine module.
        /// </summary>
        /// <param name="type">The type of module to find.</param>
        /// <returns>An instance of the loaded module.</returns>
        /// <exception cref="ModuleException">A module of the specified type was not found.</exception>
        public EngineModule GetModule(Type type)
        {
            var mod = _activeModules.FirstOrDefault(x => x.GetType() == type);

            if (mod == null)
                throw new ModuleException($"Module {type.FullName} is not loaded.");

            return mod;
        }
        
        /// <summary>
        /// Called by MonoGame when the framework initially loads.  Do not ever call this manually.
        /// </summary>
        protected override void Initialize()
        {
            // Module loader can now initialize modules.
            _reachedInit = true;

            // holds modules that need to be registered with the dev console when the console's been initialized.
            var execsToSeek = new List<object>();

            // dev console instance once we've loaded it.
            DevConsole console = null;
            
            // Speaking of which, initialize core modules.
            foreach (var mod in ModuleLoader.LoadModules(this, this.GetType().Assembly))
            {
                if (mod is DevConsole con && console == null)
                {
                    console = con;
                    while (execsToSeek.Count > 0)
                    {
                        var o = execsToSeek.First();
                        console.RegisterCommandsInternal(o);

                        execsToSeek.RemoveAt(0);
                    }
                }

                if (console == null)
                {
                    execsToSeek.Add(mod);
                }
                
                RegisterModule(mod);
            }
            
            base.Initialize();
        }

        /// <summary>
        /// Called by MonoGame when the framework has initialized but before the game loop starts.
        /// This method is used to load graphics-dependant content.  Do not call this method manually.
        /// </summary>
        protected override void LoadContent()
        {
            Blur = Content.Load<Effect>("GuiEffects/Blur");
            
            // Initialize the renderer.
            _renderer = new SpriteRocket2D(GraphicsDevice);

            // Load all third-party modules.
            foreach (var mod in ModuleLoader.LoadThirdPartyModules(this))
            {
                this.RegisterModule(mod);
            }
            
            // Tell any modules that have been loaded so far to load content.
            foreach (var mod in _activeModules)
                mod.LoadContent();

            // Future modules can load content now.
            _reachedLoadContent = true;
        }

        /// <summary>
        /// Transforms a 2D point from local space to screen space.
        /// </summary>
        /// <param name="point">The local-space point to transform.</param>
        /// <returns>The transformed screen space coordinates.</returns>
        public Vector2 PointToScreen(Vector2 point)
        {
            var baseResolution = GameUtils.BaseResolution != null
                ? GameUtils.BaseResolution.GetValueOrDefault()
                : new Vector2(_renderTarget.Width, _renderTarget.Height);

            var screenSize = new Vector2(_renderTarget.Width, _renderTarget.Height);

            return new Vector2((point.X / baseResolution.X) * screenSize.X,
                (point.Y / baseResolution.Y) * screenSize.Y);
        }
        
        /// <summary>
        /// Transforms a 2D point from screen space to local space.
        /// </summary>
        /// <param name="point">The screen-space coordinates to transform.</param>
        /// <returns>The transformed local-space coordinates.</returns>
        public Vector2 PointToLocal(Vector2 point)
        {
            var baseResolution = GameUtils.BaseResolution != null
                ? GameUtils.BaseResolution.GetValueOrDefault()
                : new Vector2(_renderTarget.Width, _renderTarget.Height);

            var screenSize = new Vector2(_renderTarget.Width, _renderTarget.Height);

            return new Vector2((point.X / screenSize.X) * baseResolution.X,
                (point.Y / screenSize.Y) * baseResolution.Y);
        }

        /// <summary>
        /// Event handler for the MonoGame exit event.  Do not call this method manually.
        /// </summary>
        /// <param name="sender">A reference to the object that generated the event being handled.</param>
        /// <param name="args">Nothing.  Literally nothing.  It's just the empty System.EventArgs object.  WHY do you need documentation on this?</param>
        /// <remarks>
        /// When ran by MonoGame, this method will unload all scenes in the Scene Stack, unload any
        /// left-over content and resources, and save the game's settings to the configuration file.
        /// </remarks>
        protected override void OnExiting(object sender, EventArgs args)
        {
            GameUtils.Log("User requested game exit.  We'll oblige.");
            
            while (_activeModules.Any())
            {
                _activeModules[0].Unload();
                _activeModules.RemoveAt(0);
            }
        }

        /// <summary>
        /// Invokes the given method on the game thread by queueing it to be ran on the next engine update.
        /// </summary>
        /// <param name="action">The method to invoke.</param>
        /// <returns>An awaitable <see cref="Task"/> that will be completed when the engine has finished executing the method.</returns>
        /// <exception cref="ArgumentNullException">The given delegate method was null.</exception>
        /// <remarks>
        /// This method is used in some areas of the engine where asynchronous code is necessary. It allows you to await a game-thread operation from
        /// asynchronous code.  This is necessary if, for example, an async method needs to read or modify the state of the GUI or otherwise instruct the engine
        /// to do something that'll munt the whole thing up if run in the middle of rendering - as the engine will wait until the next update phase to execute
        /// the task - before allowing the rest of the game's state to update.
        /// </remarks>
        public Task Invoke(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            
            var task = new Task(action);
            
            _taskQueue.Enqueue(task);

            return task;
        }
        
        /// <summary>
        /// Called by MonoGame every time the game loop ticks.  Do not call this method manually.
        /// </summary>
        /// <param name="gameTime">An object representing the elapsed time since the previous tick.</param>
        protected override void Update(GameTime gameTime)
        {
            if (_taskQueue.TryDequeue(out Task task))
            {
                task.RunSynchronously();
            }

            foreach (var module in _activeModules)
                module.Update(gameTime);
            
            base.Update(gameTime);
        }

        internal Texture2D GetFrameBuffer()
        {
            return _renderTarget;
        }
        
        /// <summary>
        /// Called by MonoGame when the game loop is about to render a frame to the screen.  Not guaranteed to run every
        /// tick.  Do not call this method manually.
        /// </summary>
        /// <param name="gameTime">An object representing the elapsed time since the last tick.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
            GraphicsDevice.Clear(Color.Black);
            
            foreach (var module in _activeModules)
                module.Draw(gameTime, _renderer);
            
            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.Black);

            _renderer.Begin(BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            _renderer.FillRectangle(new Rectangle(0, 0, (int) LocalWidth, (int) LocalHeight), Color.White,
                _renderTarget);
            
            _renderer.End();
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// Console command that exits the game ("game.exit").
        /// </summary>
        [Exec("game.exit")]
        public void Exec_Exit()
        {
            Exit();
        }
        
        /// <summary>
        /// Saves a screenshot of the last frame to disk.
        /// </summary>
        /// <remarks>
        /// This method requires the game loop to be actively running and rendering frames.  When run,
        /// it will copy the contents of the graphics device's back-buffer (and thus the previous frame)
        /// into a new texture.  This texture will then be saved as a PNG file to the game's screenshots directory
        /// with the current date and time as the file name.
        ///
        /// This method can be triggered at any time in-game by pressing F2.
        /// </remarks>
        [Exec("game.takeScreenshot")]
        public void TakeScreenshot()
        {
            // Allocate space for the back buffer data we're about to grab.
            byte[] backbuffer = new byte[Math.Abs(GraphicsDevice.PresentationParameters.BackBufferWidth * 4) * GraphicsDevice.PresentationParameters.BackBufferHeight];

            // Grab the back buffer data.  This represents the pixel data of what's on screen.
            GraphicsDevice.GetBackBufferData(backbuffer);

            // Create a texture to store the data
            var screenshotTexture = new Texture2D(GraphicsDevice, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            // Upload the data to the texture.
            screenshotTexture.SetData(backbuffer);

            // Ensure the screenshots folder exists.
            GameUtils.EnsureDirExists(GameUtils.ScreenshotsPath);

            // Save the screenshot to a PNG.
            var screenshotPath = GameUtils.GetScreenShotName();

            using (var file = File.Open(screenshotPath, FileMode.OpenOrCreate))
            {
                screenshotTexture.SaveAsPng(file, screenshotTexture.Width, screenshotTexture.Height);
            }

            // Clean up, clean up, everybody clean your RAM!
            screenshotTexture.Dispose();

            GameUtils.Log($"Screenshot saved to {screenshotPath}.");
        }
        
        /// <summary>
        /// Sets the display mode of the engine.
        /// </summary>
        /// <param name="displayMode">The screen resolution to apply.</param>
        /// <param name="fullscreenMode">A value representing the fullscreen mode to apply.</param>
        public void SetDisplayMode(DisplayMode displayMode, FullScreenMode fullscreenMode)
        {
            // fullscreen mode
            switch (fullscreenMode)
            {
                case FullScreenMode.Borderless:
                    _graphics.IsFullScreen = true;
                    _graphics.HardwareModeSwitch = false;
                    break;
                case FullScreenMode.FullScreen:
                    _graphics.IsFullScreen = true;
                    _graphics.HardwareModeSwitch = true;
                    break;
                case FullScreenMode.Windowed:
                    _graphics.IsFullScreen = false;
                    _graphics.HardwareModeSwitch = true;
                    break;
            }
            
            // resolution
            if (_graphics.HardwareModeSwitch)
            {
                _graphics.PreferredBackBufferWidth = displayMode.Width;
                _graphics.PreferredBackBufferHeight = displayMode.Height;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            }
            
            // Reallocate the render target to fit the new display mode.
            _renderTarget?.Dispose();
            _renderTarget = new RenderTarget2D(GraphicsDevice, displayMode.Width, displayMode.Height, false,
                displayMode.Format, DepthFormat.None, 4, RenderTargetUsage.PreserveContents);
            
            // applu changes.
            _graphics.ApplyChanges();
        }
        
        /// <summary>
        /// Console command that enables or disables fixed time stepping. ("game.setFixedTimeStep true|false").
        /// </summary>
        /// <param name="value">A value indicating whether fixed time stepping should be enabled.</param>
        [Exec("game.setFixedTimeStep")]
        public void SetFixedTimeStep(bool value)
        {
            IsFixedTimeStep = value;
        }
    }
}
