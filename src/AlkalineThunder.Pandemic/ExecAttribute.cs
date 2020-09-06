using System;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Exposes a method to the developer console.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ExecAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the exposed console command.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Exposes a method to the developer console.
        /// </summary>
        /// <param name="name">The name of the console command.</param>
        public ExecAttribute(string name)
        {
            Name = name;
        }
    }
}