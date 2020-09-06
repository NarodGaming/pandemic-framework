using System;

namespace AlkalineThunder.Pandemic.CommandLine.Pty
{
    /// <summary>
    /// This exception is thrown when a player interrupts a Gateway OS process (for example,
    /// when pressing CTRL+C in the middle of a command).  
    /// </summary>
    public class ProcessInterruptedException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ProcessInterruptedException" /> class.
        /// </summary>
        public ProcessInterruptedException() :
            base("A Gateway process has been interrupted by the user.")
        {}
    }
}