using System;

namespace AlkalineThunder.Pandemic.CommandLine
{
    /// <summary>
    /// Represents an exception thrown by the Gateway command shell.
    /// </summary>
    public class ShellException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ShellException"/> class.
        /// </summary>
        /// <param name="message">A message describing what the player did wrong.</param>
        public ShellException (string message) : base(message) {}
    }
}