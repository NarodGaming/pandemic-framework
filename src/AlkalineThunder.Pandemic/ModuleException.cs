using System;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Represents an exception thrown by the module loader.
    /// </summary>
    public class ModuleException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ModuleException"/>.
        /// </summary>
        /// <param name="message">A message describing why the module failed to load.</param>
        public ModuleException(string message) : base(message) {}
    }
}