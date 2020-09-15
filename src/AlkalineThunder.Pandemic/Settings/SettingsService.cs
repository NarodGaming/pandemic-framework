using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlkalineThunder.Pandemic.Gui.Controls;
using AlkalineThunder.Pandemic.Skinning;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace AlkalineThunder.Pandemic.Settings
{
    /// <summary>
    /// Provides the Pandemic Framework with a basic configuration system.
    /// </summary>
    public sealed class SettingsService : EngineModule
    {
        private const int MinimumSafeDisplayHeight = 720;
        
        private Settings Settings { get; set; }

        /// <summary>
        /// Occurs when a setting has been changed.
        /// </summary>
        public event EventHandler SettingsUpdated;

        /// <summary>
        /// Gets a value representing the current font size setting.
        /// </summary>
        public FontSizeAdjustment FontSizeAdjustment
            => Settings.FontSize;

        /// <summary>
        /// Gets or sets whether the <see cref="BackgroundBlur"/> control
        /// is allowed to blur its background UI elements or if it should just act as a translucent overlay.
        /// </summary>
        public bool EnableBlurs
        {
            get => Settings.EnableBlurs;
            set
            {
                if (Settings.EnableBlurs != value)
                {
                    Settings.EnableBlurs = value;
                    OnSettingsUpdated();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Gateway OS terminal should have a translucent background.
        /// </summary>
        public bool EnableTerminalTransparency
        {
            get => Settings.EnableTransparency;
            set
            {
                if (Settings.EnableTransparency != value)
                {
                    Settings.EnableTransparency = value;
                    OnSettingsUpdated();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the active skin used by the GUI.
        /// </summary>
        public string ActiveSkinName
        {
            get => Settings.ActiveSkinName;
            set
            {
                if (Settings.ActiveSkinName != value)
                {
                    Settings.ActiveSkinName = value ?? "default";
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the input system should swap
        /// the left and right mouse buttons.
        /// </summary>
        public bool SwapPrimaryMouseButton
        {
            get => Settings.SwapPrimaryMouseButton;
            set
            {
                if (Settings.SwapPrimaryMouseButton != value)
                {
                    Settings.SwapPrimaryMouseButton = value;
                    OnSettingsUpdated();
                }
            }
        }
        
        internal bool HighlightHoveredGuiElement
        {
            get => Settings.DevShowHoveredGuiElement;
            set
            {
                if (value != Settings.DevShowHoveredGuiElement)
                {
                    Settings.DevShowHoveredGuiElement = value;
                    OnSettingsUpdated();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the engine's <see cref="FullScreenMode"/>.
        /// </summary>
        public FullScreenMode FullScreenMode
        {
            get => Settings.FullScreenMode;
            set
            {
                Settings.FullScreenMode = value;
                OnSettingsUpdated();
            }
        }

        /// <summary>
        /// Gets or sets the percentage used to calculate the GUI scale relative to the screen's resolution.
        /// </summary>
        public float GuiScale
        {
            get => Settings.GuiScale;
            set
            {
                if (Math.Abs(Settings.GuiScale - value) > 0.0001f)
                {
                    Settings.GuiScale = value;
                    OnSettingsUpdated();
                }
            }
        }

        public void SetFontSize(FontSizeAdjustment adjustment)
        {
            if (FontSizeAdjustment != adjustment)
            {
                Settings.FontSize = adjustment;
                GetModule<SkinSystem>().ReloadSkin();
            }
        }
        
        /// <summary>
        /// Gets a value indicating the engine's current GPU display mode.
        /// </summary>
        internal DisplayMode DisplayMode
        {
            get
            {
                var resolutionString = Settings.ScreenResolution;

                if (GameUtils.ParseResolutionString(resolutionString, out int width, out int height))
                {
                    var anyMatchingMode = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.FirstOrDefault(x => x.Width == width && x.Height == height);

                    if (anyMatchingMode != null)
                    {
                        return anyMatchingMode;
                    }
                }

                var defaultMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                Settings.ScreenResolution = $"{defaultMode.Width}x{defaultMode.Height}";
                return defaultMode;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether vertical-sync and fixed time stepping is enabled.
        /// </summary>
        public bool EnableVSync
        {
            get => Settings.EnableVSync;
            set
            {
                if (Settings.EnableVSync != value)
                {
                    Settings.EnableVSync = value;
                    OnSettingsUpdated();
                }
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the UI should use the dark variant of a skin.
        /// </summary>
        public bool EnableDarkTheme
        {
            get => Settings.EnableDarkMode;
            set
            {
                Settings.EnableDarkMode = value;
                OnSettingsUpdated();
            }
        }

        /// <summary>
        /// Gets a value indicating the engine's active screen resolution.
        /// </summary>
        public string ActiveResolution => $"{DisplayMode.Width}x{DisplayMode.Height}";
        
        /// <summary>
        /// Gets a list of the engine's supported screen resolutions.
        /// </summary>
        public IEnumerable<string> AvailableResolutions => GraphicsAdapter.DefaultAdapter.SupportedDisplayModes
            .Where(x => x.Height >= MinimumSafeDisplayHeight)
            .OrderByDescending(x => x.Width * x.Height).Select(x => $"{x.Width}x{x.Height}").Distinct();
        
        /// <summary>
        /// Saves the current configuration to disk.
        /// </summary>
        public void SaveSettings()
        {
            var json = JsonConvert.SerializeObject(Settings, Formatting.Indented);
            GameUtils.EnsureDirExists(GameUtils.AppDataPath);
            File.WriteAllText(GameUtils.SettingsPath, json);
            GameUtils.Log("Settings saved.");
        }

        private void OnSettingsUpdated()
        {
            var aspectRatio = DisplayMode.Width / (float) DisplayMode.Height;
            var baseHeight = DisplayMode.Height / Settings.GuiScale;
            var baseWidth = baseHeight * aspectRatio;

            GameUtils.BaseResolution = new Vector2(baseWidth, baseHeight);

            GameLoop.SetFixedTimeStep(EnableVSync);
            GameLoop.SetDisplayMode(DisplayMode, FullScreenMode);
            SettingsUpdated?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Loads the engine configuration from disk.
        /// </summary>
        public void LoadSettings()
        {
            var appPath = GameUtils.AppDataPath;

            GameUtils.EnsureDirExists(appPath);

            GameUtils.Log("Reading settings file...");

            if (File.Exists(GameUtils.SettingsPath))
            {
                var json = File.ReadAllText(GameUtils.SettingsPath);
                Settings = JsonConvert.DeserializeObject<Settings>(json);
            }
            else
            {
                GameUtils.Log("File not found - creating a new one...");
                Settings = new Settings();
                SaveSettings();
            }
        }

        /// <summary>
        /// Applies a new screen resolution.
        /// </summary>
        /// <param name="resolution">The string representation of the resolution to apply.</param>
        /// <exception cref="NotSupportedException">The given display mode isn't supported by user's hardware.</exception>
        /// <exception cref="FormatException">The given resolution string is not of the correct format.</exception>
        [Exec("settings.setDisplayMode")]
        public void ApplyResolution(string resolution)
        {
            if (GameUtils.ParseResolutionString(resolution, out _, out _))
            {
                if (!AvailableResolutions.Contains(resolution))
                    throw new NotSupportedException(
                        "The specified resolution is not supported by the current monitor or graphics adapter.");

                this.Settings.ScreenResolution = resolution;
                this.OnSettingsUpdated();
            }
            else
            {
                throw new FormatException("Specified resolution is not in the correct format.");
            }
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            base.OnInitialize();

            LoadSettings();
        }

        /// <inheritdoc />
        protected override void OnLoadContent()
        {
            base.OnLoadContent();

            OnSettingsUpdated();
        }

        /// <inheritdoc />
        protected override void OnUnload()
        {
            base.OnUnload();
            
            SaveSettings();
        }
    }
}
