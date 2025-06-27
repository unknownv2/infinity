using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NoDev.Infinity.User;
using Newtonsoft.Json;

namespace NoDev.Infinity.Tools.Info
{
    internal class ManifestFileToolAndGameInfoRetriever : IToolInfoRetriever, IGameInfoRetriever
    {
        private IDictionary<string, GameInfo> _gameInfos;
        private IDictionary<string, ToolInfo> _toolInfos;
        private IDictionary<string, List<ToolInfo>> _byGame;

        internal ManifestFileToolAndGameInfoRetriever(string manifestFile)
        {
            LoadBoth(manifestFile);
        }

        // ReSharper disable once FunctionComplexityOverflow
        internal void LoadBoth(string manifestFile)
        {
            //try
            //{
                var root = JsonConvert.DeserializeObject<
                    Dictionary<string, Dictionary<string, Dictionary<string, object>>>
                    >(File.ReadAllText(manifestFile));

                if (!root.ContainsKey("games"))
                    _gameInfos = new Dictionary<string, GameInfo>();
                else
                {
                    var games = root["games"];

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

                _byGame = new Dictionary<string, List<ToolInfo>>();

                if (!root.ContainsKey("tools"))
                    _toolInfos = new Dictionary<string, ToolInfo>();
                else
                {
                    var tools = root["tools"];

                    _toolInfos = new Dictionary<string, ToolInfo>(tools.Count);

                    foreach (var tool in tools.Where(tool => tool.Value.ContainsKey("gameId") && tool.Value.ContainsKey("name") && tool.Value.ContainsKey("secure")))
                    {
                        var gameId = (string)tool.Value["gameId"];

                        var toolInfo = new ToolInfo(tool.Key, gameId, (string)tool.Value["name"], (AccessLevel)(long)tool.Value["access"]);

                        _toolInfos[tool.Key] = toolInfo;

                        if (_byGame.ContainsKey(gameId))
                            _byGame[gameId].Add(toolInfo);
                        else
                            _byGame.Add(gameId, new List<ToolInfo> { toolInfo });
                    }
                }
            //}
            //catch (Exception)
            //{
            //    _gameInfos = new Dictionary<string, GameInfo>();
            //    _toolInfos = new Dictionary<string, ToolInfo>();
            //    _byGame = new Dictionary<string, List<ToolInfo>>();
            //}
        }

        public ToolInfo RetrieveToolInfo(string toolId)
        {
            return _toolInfos.GetOrDefault(toolId);
        }

        public IEnumerable<ToolInfo> RetrieveToolInfoByGameId(string gameId)
        {
            return _byGame.ContainsKey(gameId) ? _byGame[gameId].AsReadOnly() : Enumerable.Empty<ToolInfo>();
        }

        public GameInfo RetrieveGameInfo(string gameId)
        {
            return _gameInfos.GetOrDefault(gameId);
        }

        public IEnumerable<GameInfo> RetrieveGameInfo()
        {
            return _gameInfos.Values;
        }
    }
}
