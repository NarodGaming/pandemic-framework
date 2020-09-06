using System;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Represents Michael's complete frustration in his futile attempts tp stop his fellow human
    /// brethren from somehow managing to get the Pandemic Framework to break in such a right way that it throws
    /// this exception.  If this exception is EVER thrown, SOMEONE - probably you - is an IDIOT.
    /// </summary>
    public class CompleteAndTotalFuckingIdiotDeveloperException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CompleteAndTotalFuckingIdiotDeveloperException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public CompleteAndTotalFuckingIdiotDeveloperException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public override string HelpLink { get => "https://youtu.be/dQw4w9WgXcQ"; set => throw new CompleteAndTotalFuckingIdiotDeveloperException("Fuck you."); }
    }
}