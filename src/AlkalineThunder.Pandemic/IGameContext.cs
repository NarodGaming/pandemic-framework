using System;

namespace AlkalineThunder.Pandemic
{
    /// <summary>
    /// Represents an object that acts as a Pandemic Framework game context.
    /// </summary>
    public interface IGameContext
    {
        /// <summary>
        /// Gets an instance of the engine's game loop.
        /// </summary>
        public GameLoop GameLoop { get; }

        /// <summary>
        /// Gets a module of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="EngineModule"/> to find.</typeparam>
        /// <returns>An instance of the specified type.</returns>
        T GetModule<T>() where T : EngineModule, new();
        
        /// <summary>
        /// Finds an engine module of the specified type.
        /// </summary>
        /// <param name="type">The type of module to find.</param>
        /// <returns>An <see cref="EngineModule"/> of the given type if found, null if not found.</returns>
        EngineModule GetModule(Type type);
    }
}