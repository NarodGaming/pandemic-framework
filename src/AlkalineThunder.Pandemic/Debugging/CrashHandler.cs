using System;
using System.IO;
using System.Text;

namespace AlkalineThunder.Pandemic.Debugging
{
    /// <summary>
    /// Provides the engine with a way to gracefully shut down and provide bug report information
    /// when the game crashes.
    /// </summary>
    public class CrashHandler : EngineModule
    {
        private string _crashDetectionFile = "__last-crash.txt";
        
        /// <summary>
        /// Indicates the path to the folder where crash data is stored.
        /// </summary>
        public string CrashLogsFolder => Path.Combine(GameUtils.AppDataPath, "crashes");
        
        /// <summary>
        /// Indicates whether the game crashed when it was previously run.
        /// </summary>
        public bool WasPreviousCrashDetected { get; private set; }
        
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            GameUtils.Log("Crash Handler is starting up.");
            GameUtils.EnsureDirExists(CrashLogsFolder);

            var detectPath = Path.Combine(CrashLogsFolder, _crashDetectionFile);

            if (File.Exists(detectPath))
            {
                File.Delete(detectPath);
                WasPreviousCrashDetected = true;
            }
            else
            {
                WasPreviousCrashDetected = false;
            }
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            GameUtils.Log("Armed and ready.");
            
            base.OnInitialize();
        }

        private void HandleCrash(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"{GameUtils.GameTitle.ToUpper()} - FATAL CRASH REPORT");
            sb.AppendLine($"{DateTime.UtcNow.ToShortDateString()} at {DateTime.UtcNow.ToShortTimeString()} (UTC)");
            sb.AppendLine("=============================================");
            
            sb.AppendLine();

            sb.AppendLine("What happened?");
            sb.AppendLine("--------------");
            sb.AppendLine();
            sb.AppendLine(
                "Unfortunately, the game has experienced a fatal error that required the game to exit.  This is most likely due to a bug in the game.  Please attach this file in full if you decide to submit a bug report.");

            sb.AppendLine();

            sb.AppendLine(
                "If the game crashed during an active session, the game has attempted to save your progress.  Since we didn't expect this to happen, there's a chance that saving failed, or worse, that the save file is now corrupt.  While corruption is rare, it can happen - and we apologize in advance.");

            sb.AppendLine();

            sb.AppendLine("Technical information");
            sb.AppendLine("----------------------");

            sb.AppendLine();

            sb.AppendLine($"{ex}");

            foreach (var line in sb.ToString().Split(Environment.NewLine))
                GameUtils.Log(line);

            File.WriteAllText(Path.Combine(CrashLogsFolder, _crashDetectionFile), sb.ToString());
            File.WriteAllText(Path.Combine(CrashLogsFolder, GetCrashName()), sb.ToString());
        }

        private string GetCrashName()
        {
            var rawName = $"crash-{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}.txt";

            var sane = "";

            foreach (var c in rawName)
            {
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' || c == ' ')
                {
                    sane += c;
                }
                else
                {
                    if (!sane.EndsWith('-'))
                        sane += '-';
                }
            }

            return sane;
        }
        
        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            GameUtils.Log("Crap.  CRAP.  SLAM ON THE BREAKS.  WE'RE ABOUT TO CRAAAAAAAAAAAAASH");

            HandleCrash(e.ExceptionObject as Exception);

            GameLoop.Exit();
        }
    }
}