using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;

namespace NoDev.Infinity.Tools
{
    internal class ManifestFileGameInfoRetriever : IGameInfoRetriever
    {
        private IDictionary<string, GameInfo> _gameInfos;

        internal ManifestFileGameInfoRetriever(string manifestFile)
        {
            Load(manifestFile);
        }

        public GameInfo RetrieveGameInfo(string gameId)
        {
            return _gameInfos.GetOrDefault(gameId);
        }

        public IEnumerable<GameInfo> RetrieveGameInfo()
        {
            return _gameInfos.Values;
        }

        internal void Load(string manifestFile)
        {
            if (!File.Exists(manifestFile))
            {
                _gameInfos = new Dictionary<string, GameInfo>();
                return;
            }

            try
            {
                var games = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(manifestFile));

                _gameInfos = new Dictionary<string, GameInfo>(games.Count);

                foreach (var game in games)
                {
                    var gameName = game.Key;

                    if (game.Value.ContainsKey("name"))
                        try { gameName = (string)game.Value["name"]; }
                        catch { /* ignored */ }

                    Image thumbnail = null;

                    if (game.Value.ContainsKey("thumbnail") && game.Value["thumbnail"] != null)
                        try { thumbnail = Convert.FromBase64String((string)game.Value["thumbnail"]).ToImage(); }
                        catch { /* ignored */ }

                    _gameInfos[game.Key] = new GameInfo(game.Key, gameName, thumbnail);
                }
            }
            catch
            {
                _gameInfos = new Dictionary<string, GameInfo>();
            }
        }
    }
}
