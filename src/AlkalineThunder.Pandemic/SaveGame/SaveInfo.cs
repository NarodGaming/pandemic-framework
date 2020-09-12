using System;
using System.IO;
using System.Text;

namespace AlkalineThunder.Pandemic.SaveGame
{
    /// <summary>
    /// Contains the metadata and header information of a SARS-formatted Socially Distant binary save file.
    /// </summary>
    /// <remarks>
    /// We get it - naming a save file format after a disastrous real-life global pandemic is a LITTLE BIT evil and depressing.  But what did you expect?
    /// The developer of this game has a really dark sense of humour, is affected by the global pandemic just as much as you are, for some reason is typing
    /// this in third-person despite being the very person who wrote this remark, and is making a game that's literally about a global pandemic that causes
    /// a much more deadly version of the COVID-19 disease.  So deal with it.
    /// </remarks>
    public sealed class SaveInfo
    {
        /// <summary>
        /// Gets or sets the name of the player, which is displayed in the Main Meu and Continue Game menu.
        /// </summary>
        public string PlayerName { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the last completed Mission.
        /// </summary>
        public string LastMissionName { get; set; }
        
        /// <summary>
        /// Gets or sets the last time the save was written to, a.k.a the last time the game was played.
        /// </summary>
        public DateTime LastPlayed { get; set; }
        
        /// <summary>
        /// Gets or sets the first time the save was written to, a.k.a the creation date.
        /// </summary>
        public DateTime Created { get; set; }
        
        private SaveInfo() {}

        /// <summary>
        /// Serializes the data of this <see cref="SaveInfo"/> into a binary SARS header.
        /// </summary>
        /// <returns>The binary data represented by this <see cref="SaveInfo"/> object.</returns>
        public byte[] ToBlob()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
                {
                    writer.Write(Created.Ticks);
                    writer.Write(LastPlayed.Ticks);
                    writer.Write(PlayerName);
                    writer.Write(LastMissionName);
                }

                return ms.ToArray();
            }
        }

        internal static SaveInfo MakeNew(string playerName)
        {
            var info = new SaveInfo();

            info.PlayerName = playerName;
            info.LastMissionName = "";

            info.Created = DateTime.Now;
            info.LastPlayed = DateTime.Now;
            
            return info;
        }
        
        /// <summary>
        /// Creates a new <see cref="SaveInfo"/> instance from the raw SARS save header data.
        /// </summary>
        /// <param name="blob">A byte array containing the raw SARS data to parse.</param>
        /// <returns>The <see cref="SaveInfo"/> object represented within the given SARS data.</returns>
        public static SaveInfo FromBlob(byte[] blob)
        {
            var info = new SaveInfo();

            using (var ms = new MemoryStream(blob))
            {
                using (var reader = new BinaryReader(ms, Encoding.UTF8))
                {
                    info.Created = new DateTime(reader.ReadInt64());
                    info.LastPlayed = new DateTime(reader.ReadInt64());
                    info.PlayerName = reader.ReadString();
                    info.LastMissionName = reader.ReadString();
                }
            }
            
            return info;
        }
    }
}