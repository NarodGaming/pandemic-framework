using System;

namespace AlkalineThunder.Pandemic.SaveGame
{
    /// <summary>
    /// Contains information about a single save file.
    /// </summary>
    public struct SaveSlot
    {
        /// <summary>
        /// Contains the slot's identifier.
        /// </summary>
        public string Slot;
        
        /// <summary>
        /// Contains the save game's player name.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// Contains the time at which the game was last played.
        /// </summary>
        public DateTime LastPlayed;
        
        /// <summary>
        /// Contains the time at which the game was first played.
        /// </summary>
        public DateTime Created;

        /// <summary>
        /// Creates a new instance of the <see cref="SaveSlot"/> structure.
        /// </summary>
        /// <param name="slot">The slot identifier.</param>
        /// <param name="name">The player name for the save.</param>
        /// <param name="lastPlayed">The time at which the game was last played.</param>
        /// <param name="created">The time at which the game was first played.</param>
        public SaveSlot(string slot, string name, DateTime lastPlayed, DateTime created)
        {
            Name = name;
            Slot = slot;
            Created = created;
            LastPlayed = lastPlayed;
        }
    }
}