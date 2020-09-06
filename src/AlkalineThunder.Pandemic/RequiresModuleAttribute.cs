using System;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Marks a class as requiring an <see cref="EngineModule"/> to be loaded before the engine can load
    /// the object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequiresModuleAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of required engine module.
        /// </summary>
        public Type RequiredModuleType { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="EngineModule"/> class.
        /// </summary>
        /// <param name="requirement">The type of engine module that's required.</param>
        public RequiresModuleAttribute(Type requirement)
        {
            RequiredModuleType = requirement;
        }
    }
}