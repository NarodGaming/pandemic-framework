using System;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using LiteDB;
using System.Collections.Generic;
using AlkalineThunder.Pandemic;

namespace AlkalineThunder.Pandemic.SaveGame
{
    /// <summary>
    /// Provides the game with a database-driven save system.
    /// </summary>
    public class SaveSystem : EngineModule
    {
        private object _mutex = new object();
        private static readonly byte[] SaveMagic = Encoding.UTF8.GetBytes("COVID-19");
        private SaveInfo _saveInfo;
        private string _slot;
        private MemoryStream _dataStream;
        private Dictionary<string, SaveInfo> _slots = new Dictionary<string, SaveInfo>();
        private SaveFile _activeTransaction = null;
        private Random _random;

        public Random Random => _random;
        
        public SaveFile OpenSaveFile()
        {
            if (_activeTransaction != null && !_activeTransaction.IsDisposed)
            {
                throw new InvalidOperationException("Save file is already in use.");
            }

            if (!IsGameLoaded)
                throw new InvalidOperationException("Save game is not currently active.");

            _activeTransaction = new SaveFile(_dataStream);
            return _activeTransaction;
        }
        
        /// <summary>
        /// Occurs when a save game has been unloaded.
        /// </summary>
        public event EventHandler SaveUnloaded;
        
        /// <summary>
        /// Gets a value indicating whether a game is currently active.
        /// </summary>
        public bool IsGameLoaded => _saveInfo != null;

        public bool IsSaveInUse => _activeTransaction != null && !_activeTransaction.IsDisposed;
        
        private string SavesFolder => Path.Combine(GameUtils.AppDataPath, "saves");
        
        /// <summary>
        /// Gets a value representing the player's name.
        /// </summary>
        public string PlayerName => _saveInfo?.PlayerName;

        /// <summary>
        /// Occurs when a save game has been loaded.
        /// </summary>
        public event EventHandler FeedUpdated;
        
        /// <summary>
        /// Unloads the current game, if any, saving it to disk by default.
        /// </summary>
        /// <param name="save">Whether the current game should be saved to disk before unloading.</param>
        [Exec("saves.unload")]
        public void UnloadGame(bool save = true)
        {
            if (IsGameLoaded)
            {
                if (save)
                    SaveGame();

                _random = null;
                
                _dataStream.Dispose();
                _saveInfo = null;

                SaveUnloaded?.Invoke(this, EventArgs.Empty);
            }
        }
        
        private void LookForExistingSaves()
        {
            lock (_mutex)
            {
                _slots.Clear();

                foreach (var file in Directory.GetFiles(SavesFolder))
                {
                    var fileInfo = new FileInfo(file);
                    var slotName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(".", StringComparison.Ordinal));

                    using (var stream = File.OpenRead(file))
                    {
                        ThrowIfInvalidFormat(stream);

                        using (var reader = new BinaryReader(stream, Encoding.UTF8))
                        {
                            var header = ReadSaveHeader(reader);

                            _slots.Add(slotName, header);
                        }
                    }
                }
            }
        }
        
        private string GetPath(string name)
        {
            return Path.Combine(SavesFolder, name + ".sars");
        }
        
        private SaveInfo ReadSaveHeader(BinaryReader reader)
        {
            var headerLength = reader.ReadInt32();
            var headerBlob = reader.ReadBytes(headerLength);

            return SaveInfo.FromBlob(headerBlob);
        }
        
        private void ThrowIfInvalidFormat(Stream stream)
        {
            var magic = new byte[SaveMagic.Length];
            stream.Read(magic, 0, magic.Length);
            
            if (!magic.SequenceEqual(SaveMagic))
                throw new InvalidOperationException("Save file is not in a valid format.");
        } 
        
        private void OpenMemoryDatabase(BinaryReader reader)
        {
            var dbLength = reader.ReadInt32();
            var dbBlob = reader.ReadBytes(dbLength);

            using var ms = new MemoryStream(dbBlob);
            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
            _dataStream = new MemoryStream();

            gzip.CopyTo(_dataStream);
                    
            _dataStream.Position = 0;
        }

        /// <summary>
        /// Creates a new game with the given player name.
        /// </summary>
        /// <param name="playerName">The name of the player.  We'd suggest "Ash" as a name.</param>
        /// <exception cref="InvalidOperationException">A game is already in progress.</exception>
        [Exec("saves.new")]
        public void NewGame(string playerName)
        {
            var slotName = "";
            
            lock (_mutex)
            {
                if (IsGameLoaded)
                    throw new InvalidOperationException("A game is already active.");

                var saveInfo = SaveInfo.MakeNew(playerName);
                slotName = GetNextSlotName();
                var savePath = GetPath(slotName);

                _slots.Add(slotName, saveInfo);

                using (var stream = File.Open(savePath, FileMode.OpenOrCreate))
                {
                    using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(SaveMagic);

                        var headerBlob = saveInfo.ToBlob();

                        writer.Write(headerBlob.Length);
                        writer.Write(headerBlob);

                        using (var ms = new MemoryStream())
                        {
                            var db = new LiteDatabase(ms);
                            
                            db.Dispose();

                            var dbBytes = ms.ToArray();

                            using (var gzipTarget = new MemoryStream())
                            {
                                using (var gzip = new GZipStream(gzipTarget, CompressionLevel.Optimal))
                                {
                                    gzip.Write(dbBytes, 0, dbBytes.Length);
                                }

                                var zippedData = gzipTarget.ToArray();

                                writer.Write(zippedData.Length);
                                writer.Write(zippedData);
                            }
                        }
                    }
                }
            }

            LoadGame(slotName);
        }

        private string GetNextSlotName()
        {
            var id = 1;
            var slotName = "";

            do
            {
                slotName = $"SARS-CoV-2_{id}";
                id++;
            } while (_slots.ContainsKey(slotName));
            
            return slotName;
        }
        
        /// <summary>
        /// Loads a game from the specified slot.
        /// </summary>
        /// <param name="name">The identifier for the slot to load.</param>
        /// <exception cref="InvalidOperationException">A game is already in progress, or the specified slot wasn't found.</exception>
        [Exec("saves.load")]
        public void LoadGame(string name)
        {
            lock (_mutex)
            {
                GameUtils.Log("loading save game in slot " + name);

                if (IsGameLoaded)
                    throw new InvalidOperationException("A save game is already loaded and active.");

                var path = GetPath(name);

                if (!File.Exists(path))
                    throw new InvalidOperationException("Specified save game does not exist.");

                using (var stream = File.OpenRead(path))
                {
                    ThrowIfInvalidFormat(stream);

                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        _saveInfo = ReadSaveHeader(reader);

                        OpenMemoryDatabase(reader);

                        _random = new Random(PlayerName.GetHashCode());
                    }
                }

                _saveInfo.LastPlayed = DateTime.Now;

                _slot = name;
            }
        }

        /// <summary>
        /// Saves the current game.
        /// </summary>
        /// <exception cref="InvalidOperationException">The current game isn't a thing that exists.</exception>
        [Exec("saves.save")]
        public void SaveGame()
        {
            lock (_mutex)
            {
                if (!IsGameLoaded)
                    throw new InvalidOperationException("There's no game to save.");

                var slotPath = GetPath(_slot);

                GameUtils.Log($"Saving current game to {slotPath}.");

                using var stream = File.Open(slotPath, FileMode.OpenOrCreate);
                using var writer = new BinaryWriter(stream, Encoding.UTF8);

                writer.Write(SaveMagic);

                var headerBlob = _saveInfo.ToBlob();

                writer.Write(headerBlob.Length);
                writer.Write(headerBlob);

                using var zipBuffer = new MemoryStream();

                using (var zippo = new GZipStream(zipBuffer, CompressionLevel.Optimal, true))
                {
                    if (IsSaveInUse)
                        _activeTransaction.Cancel();

                    var dbBytes = _dataStream.ToArray();
                    zippo.Write(dbBytes, 0, dbBytes.Length);

                    _activeTransaction = null;
                }

                var zippedData = zipBuffer.ToArray();
                writer.Write(zippedData.Length);
                writer.Write(zippedData);

                GameUtils.Log("Game saved.");
            }
        }
        
        /// <summary>
        /// Retrieves a list of all available save slots.
        /// </summary>
        /// <returns>A list of all available save slots. Can't you read the summary, damnit?</returns>
        public IEnumerable<SaveSlot> GetSlots()
        {
            LookForExistingSaves();

            lock (_mutex)
            {
                foreach (var slot in _slots.Keys)
                {
                    var saveInfo = _slots[slot];
                    yield return new SaveSlot(slot, saveInfo.PlayerName, saveInfo.LastPlayed, saveInfo.Created);
                }
            }
        }
        
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            base.OnInitialize();

            GameUtils.EnsureDirExists(SavesFolder);
        }

        /// <inheritdoc />
        protected override void OnLoadContent()
        {
            base.OnLoadContent();
            
            LookForExistingSaves();
        }
        
        /// <inheritdoc />
        protected override void OnUnload()
        {
            base.OnUnload();
            
            if (IsGameLoaded)
                SaveGame();
        }

        public class SaveFile : IDisposable
        {
            private bool _isDisposed = false;
            private LiteDatabase _db = null;
            private Stream _dbStream = null;
            
            public LiteDatabase Database => _db ?? throw new ObjectDisposedException("SaveFile");

            public bool IsDisposed => _isDisposed;
            
            public SaveFile(Stream dataStream)
            {
                _dbStream = dataStream ?? throw new ArgumentNullException(nameof(dataStream));
                _db = new LiteDatabase(_dbStream);
            }
            
            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _isDisposed = true;
                    _db.Dispose();
                    _dbStream = null;
                    _db = null;
                }
            }

            public void Cancel()
            {
                if (!_isDisposed)
                {
                    _db = null;
                    _dbStream = null;
                    _isDisposed = true;
                }
            }
        }
    }
}