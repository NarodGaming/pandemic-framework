using System.Collections.Generic;
using AlkalineThunder.Pandemic.Gui.Controls;

namespace AlkalineThunder.Pandemic.CommandLine
{
    /// <summary>
    /// Provides a <see cref="ConsoleControl"/> with a dynamic list of available tab completions.
    /// </summary>
    public interface ITabCompletionSource
    {
        /// <summary>
        /// Gets a list of possible tab completions.
        /// </summary>
        IEnumerable<string> AvailableCompletions { get; }
    }
}