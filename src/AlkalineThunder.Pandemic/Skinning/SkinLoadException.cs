using System;

namespace AlkalineThunder.Pandemic.Skinning
{
    /// <summary>
    /// Represents an exception thrown when a skin fails to load.
    /// </summary>
    public class SkinLoadException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SkinLoadException"/> class.
        /// </summary>
        /// <param name="message">A message that describes why the skin couldn't be loaded.</param>
        public SkinLoadException(string message) : base(message) {}
        
        /// <summary>
        /// Creates a new instance of the <see cref="SkinLoadException"/> class.
        /// </summary>
        /// <param name="message">A message that describes why the skin failed to load.</param>
        /// <param name="inner">An inner exception containing more information about why the skin failed to load.</param>
        public SkinLoadException(string message, Exception inner) : base(message, inner) {}
    }
}