using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Provides commonly-used functions and properties that are used all throughout the game and it's engine.
    /// </summary>
    public static class GameUtils
    {
        private static Assembly _gameAssembly;
        
        /// <summary>
        /// A friendly identifier that signifies whether this is a debug or shipping/release
        /// version of Socially Distant.
        /// </summary>
#if DEBUG
        public static readonly string BuildVariant = "Debug";
#else
        public static readonly string BuildVariant = "Shipping";
#endif
        
        /// <summary>
        /// Gets or sets the base resolution of the engine's viewport.  If null, the game will
        /// match the resolution of the configuration file and no scaling will be applied whatsoever.
        /// </summary>
        public static Vector2? BaseResolution { get; set; } = null;
        
        private static Assembly Assembly => _gameAssembly;

        /// <summary>
        /// Gets a value representing the path to the folder where the game stores all of it's
        /// player-specific data.
        /// </summary>
        public static string AppDataPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AuthorName, GameTitle);

        /// <summary>
        /// Gets a value representing the path to the game's configuration file.
        /// </summary>
        public static string SettingsPath => Path.Combine(AppDataPath, "Settings.json");

        /// <summary>
        /// Gets a value representing the path to the game's screenshots folder.
        /// </summary>
        public static string ScreenshotsPath => Path.Combine(AppDataPath, "Screenshots");

        /// <summary>
        /// Gets a value representing the current developer of the game.
        /// </summary>
        public static string AuthorName
        {
            get
            {
                var attribute = Assembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;

                return attribute != null ? attribute.Company : string.Empty;
            }
        }

        /// <summary>
        /// Gets a value representing the build version of the game.
        /// </summary>
        public static string Build
        {
            get
            {
                var version = Assembly.GetName().Version;
                if (version != null) return version.ToString();
                return "<unknown>";
            }
        }

        /// <summary>
        /// Gets a value representing the game's current title.
        /// </summary>
        public static string GameTitle
        {
            get
            {
                var attribute = Assembly.GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;

                return attribute != null ? attribute.Product : string.Empty;
            }
        }

        internal static Queue<string> LogStack = new Queue<string>();
        
        /// <summary>
        /// Prints a formatted message to the game's console.
        /// </summary>
        /// <param name="message">The text of the message to write.</param>
        /// <param name="lineNumber">The line number in the code where the method was called. Will be automatically populated by the compiler.</param>
        /// <param name="memberName">The name of the method or property that called this method. Will be automatically populated by the compiler.</param>
        /// <param name="path">The path to the source code file that called this method. Will be automatically populated by the compiler. Will only be printed in debug builds.</param>
        public static void Log(string message, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "", [CallerFilePath] string path = "")
        {
            var date = DateTime.UtcNow;

#if DEBUG
            string logLine = $"[{date.ToShortDateString()} {date.ToShortTimeString()}] <{path}:{lineNumber}> <{memberName}()> {message}";

            Debug.Print(logLine);
#else
            string logLine = $"[{date.ToShortDateString()} {date.ToShortTimeString()}] <{lineNumber}:{memberName}()> {message}";
#endif

            LogStack.Enqueue(logLine);
            Console.WriteLine(logLine);
        }

        /// <summary>
        /// Creates the specified directory if it does not already exist.
        /// </summary>
        /// <param name="path">The path to the directory to create.</param>
        public static void EnsureDirExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Log($"Creating: {path}");
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Nukes the contents of the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory to nuke.</param>
        /// <remarks>
        /// If the given path exists as a directory, it will be recursively deleted.  It will then be
        /// re-created as an empty directory using <see cref="EnsureDirExists"/>.
        /// </remarks>
        public static void Nuke(string path)
        {
            if (Directory.Exists(path))
            {
                Log($"Nuking directory: {path}");
                Directory.Delete(path, true);
            }

            EnsureDirExists(path);
        }

        /// <summary>
        /// Tries to parse a screen resolution string and extracts the width and height.
        /// </summary>
        /// <param name="resolution">The resolution string to parse.</param>
        /// <param name="width">The extracted width.</param>
        /// <param name="height">The extracted height.</param>
        /// <returns>A value indicating whether the resolution string was successfully parsed.</returns>
        /// <remarks>
        /// All resolution strings must be in the form of "WIDTHxHEIGHT", where WIDTH and HEIGHT are both
        /// positive integers.  Valid examples include "1920x1080", "640x480", "1366x768", and "2560x1440".
        /// The <paramref name="resolution"/> string will be matched against the following regular expression:
        /// <code>/^([0-9])x([0-9])$/</code>.
        /// </remarks>
        public static bool ParseResolutionString(string resolution, out int width, out int height)
        {
            // Set invalid values in case we need to break out with an error. This method is NOT allowed
            // to throw exceptions.
            width = -1;
            height = -1;

            // Empty or whitespace strings definitely won't match.
            if (string.IsNullOrWhiteSpace(resolution))
                return false;

            // This seems like something regex can do.
            var match = Regex.Match(resolution, "([0-9]+)x([0-9]+)");
            
            if (match.Success)
            {
                var widthGroup = match.Groups[1];
                var heightGroup = match.Groups[2];

                var widthText = resolution.Substring(widthGroup.Index, widthGroup.Length);
                var heightText = resolution.Substring(heightGroup.Index, heightGroup.Length);

                width = int.Parse(widthText);
                height = int.Parse(heightText);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a file name for a new screenshot.
        /// </summary>
        /// <returns>The new screenshot file name.</returns>
        /// <remarks>
        /// This method will generate a file name starting with the word "Screenshot," with the
        /// current date and time appended to it.  The file extension will always be ".png".
        /// </remarks>
        public static string GetScreenShotName()
        {
            var path = ScreenshotsPath;
            var date = DateTime.UtcNow;

            var filename = $"Screenshot {date.ToShortDateString().Replace("/", "-")} {date.ToLongTimeString().Replace(":", ".")}.png";

            return Path.Combine(path, filename);
        }

        /// <summary>
        /// Parses an HTML color code and returns the resulting RGBA color.
        /// </summary>
        /// <param name="hex">The HTML color code to parse.</param>
        /// <returns>The resulting RGBA color represented by the string.</returns>
        /// <exception cref="FormatException">Thrown if the given HTML color string is of an invalid format.</exception>
        /// <remarks>
        /// Will parse both named colors ("red", "white", "blue", etc) and hexadecimal colors ("#ffffff").  Hexadecimal colors
        /// can either be three digits ("#fff") for 12 bits of color depth, 6 digits ("#ffffff") for 24 bits of color depth,
        /// or 8 digits ("#ffffffff") for 24 bits of color depth plus an 8-bit alpha channel.  In any case, all hexadecimal
        /// color values are in the order of "RGBA".
        /// </remarks>
        public static Color ParseHexColor(string hex)
        {
            if (TryParseHexColor(hex, out Color color))
                return color;
            else
                throw new FormatException("Color value was not in the correct format.");
        }
        
        /// <summary>
        /// Tries to parse an HTML color code.
        /// </summary>
        /// <param name="hex">The color code to parse.</param>
        /// <param name="color">The RGBA color represented by the HTML color string.</param>
        /// <returns>Whether the HTML color code was of a valid format.</returns>
        /// <remarks>
        /// Will parse both named colors ("red", "white", "blue", etc) and hexadecimal colors ("#ffffff").  Hexadecimal colors
        /// can either be three digits ("#fff") for 12 bits of color depth, 6 digits ("#ffffff") for 24 bits of color depth,
        /// or 8 digits ("#ffffffff") for 24 bits of color depth plus an 8-bit alpha channel.  In any case, all hexadecimal
        /// color values are in the order of "RGBA".
        /// </remarks>
        public static bool TryParseHexColor(string hex, out Color color)
        {
            try
            {
                var gdiColor = System.Drawing.ColorTranslator.FromHtml(hex);
                
                color = new Color(gdiColor.R, gdiColor.G, gdiColor.B, gdiColor.A);
                return true;
            }
            catch
            {
                color = Color.Transparent;
                return false;
            }
        }
        
        /// <summary>
        /// Lightens the given color by the given percentage.
        /// </summary>
        /// <param name="color">The original color to lighten.</param>
        /// <param name="amount">The amount to lighten the color by.</param>
        /// <returns>The lightened color value.</returns>
        public static Color Lighten(this Color color, float amount)
        {
            var hsl = HslColor.HslFromRgb(color);

            hsl.Luminance *= 1 + amount;
            
            return HslColor.HslToColor(hsl);
        }
        
        /// <summary>
        /// Darkens the given color by the given percentage.
        /// </summary>
        /// <param name="color">The original color to darken.</param>
        /// <param name="amount">The amount to darken the color by.</param>
        /// <returns>The darkened color value.</returns>
        public static Color Darken(this Color color, float amount)
        {
            var hsl = HslColor.HslFromRgb(color);

            hsl.Luminance *= 1 - amount;
            
            return HslColor.HslToColor(hsl);
        }

        public static void Run(Assembly gameAssembly)
        {
            _gameAssembly = gameAssembly ?? throw new ArgumentNullException(nameof(gameAssembly));
            
            Log($"Starting {GameTitle}...");
            Log($"A game developed by {AuthorName}.");
            Log("Powered by the Pandemic Framework.");
            Log("");
            Log("============================================");
            Log("");
            
            using var game = new GameLoop();

            game.Run();

            Log("The pandemic is now over.");
        }
    }
}
