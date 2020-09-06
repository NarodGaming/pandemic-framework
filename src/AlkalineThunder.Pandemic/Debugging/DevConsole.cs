using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AlkalineThunder.Pandemic.Gui;
using AlkalineThunder.Pandemic.Gui.Controls;
using AlkalineThunder.Pandemic.Scenes;
using Microsoft.Xna.Framework;

namespace AlkalineThunder.Pandemic.Debugging
{
    /// <summary>
    /// Provides the engine with a simple pull-down developer console.
    /// </summary>
    [RequiresModule(typeof(SceneSystem))]
    public sealed class DevConsole : EngineModule
    {
        private bool _isOpen;
        private Box _overlay;
        private Box _consoleHolder;
        private ConsoleControl _console;
        private Task _consoleTask;
        private bool _printStackTraces;

        private SceneSystem SceneSystem
            => GetModule<SceneSystem>();
        
        /// <summary>
        /// Indicates whether the console is open.
        /// </summary>
        public bool IsOpen => _isOpen;
        
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            _overlay = new Box
            {
                BackgroundColor = (Color.Black * 0.5f),
                ContentVerticalAlignment = VerticalAlignment.Top
            };

            _consoleHolder = new Box
            {
                FixedHeight = 400
            };

            _overlay.Content = _consoleHolder;

            _console = new ConsoleControl
            {
                DoShellInterrupts = false
            };
            _consoleHolder.Content = _console;

            _consoleHolder.Click += (o, a) =>
            {
                // GameLoop.SceneSystem.SetFocus(_console);
            };
            
            _overlay.Click += (o, a) =>
            {
                Close();
            };
        }
        
        /// <summary>
        /// Opens the developer console.
        /// </summary>
        public void Open()
        {
            _isOpen = true;

            var cscene = SceneSystem.PushScene<ConsoleScene>();
            cscene.Gui.AddChild(_overlay);
            SceneSystem.SetFocus(_console);
            
            // GameLoop.SceneSystem.AddToCanvas(_overlay);
            // GameLoop.SceneSystem.SetFocus(_console);

            if (_consoleTask == null || _consoleTask.IsFaulted)
            {
                _consoleTask = DevConsoleShell(_console.Output, _console.Input);
            }
        }

        /// <summary>
        /// Closes the developer console.
        /// </summary>
        [Exec("exit")]
        public void Close()
        {
            _isOpen = false;
            SceneSystem.UnloadScene<ConsoleScene>();
        }

        /// <summary>
        /// Prints to the developer console.
        /// </summary>
        /// <param name="text">The text to wwrite to the console.</param>
        public void WriteLine(string text)
        {
            if (_console != null)
            {
                _console.WriteLine(text);
            }
        } 
        
        private async Task DevConsoleShell(StreamWriter output, StreamReader input)
        {
            var running = true;
            do
            {
                await output.WriteAsync(">>> ");

                var line = await input.ReadLineAsync();

                try
                {
                    await GameLoop.ExcecuteCommand(line, output, input);
                }
                catch (TargetInvocationException ex)
                {
                    if (_printStackTraces)
                    {
                        await output.WriteLineAsync(ex.ToString());
                    }
                    else
                    {
                        await output.WriteLineAsync($"Error: {ex.InnerException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    if (_printStackTraces)
                    {
                        await output.WriteLineAsync(ex.ToString());
                    }
                    else
                    {
                        await output.WriteLineAsync($"Error: {ex.Message}");
                    }
                }
            } while (running);
        }

        /// <summary>
        /// Sets whether the console should orint full stack traces when errors occur in commands.
        /// This method is executable with "console.printStackTraces".
        /// </summary>
        /// <param name="value">Whether the console should print full stack traces for errors.</param>
        [Exec("console.printStackTraces")]
        public void SetPrintStackTraces(bool value)
        {
            _printStackTraces = value;
        }

        /// <inheritdoc />
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (_isOpen)
            {
                if (GameUtils.LogStack.Count > 0)
                {
                    while (GameUtils.LogStack.Count > 0)
                        WriteLine(GameUtils.LogStack.Dequeue());
                    _console.Write(">>> ");
                }
            }
            
            if (_isOpen && _overlay.Scene == null)
                _isOpen = false;
        }

        private class ConsoleScene : Scene
        {
            
        }
    }
}