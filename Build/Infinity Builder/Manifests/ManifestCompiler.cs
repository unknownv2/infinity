using System.Collections.Generic;
using Newtonsoft.Json;

namespace NoDev.Infinity.Build.InfinityBuilder.Manifests
{
    internal class ManifestCompiler : IManifestCompiler
    {
        public string CompileForClient(GameManifest gameManifest, ToolManifest toolManifest)
        {
            var gameList = new Dictionary<string, Dictionary<string, object>>(gameManifest.Count);

            foreach (var game in gameManifest)
            {
                gameList.Add(game.Key, new Dictionary<string, object>
                {
                    { "name", game.Value.Name },
                    { "thumbnail", game.Value.Thumbnail }
                });
            }

            var toolList = new Dictionary<string, Dictionary<string, object>>(toolManifest.Count);

            foreach (var tool in toolManifest)
            {
                toolList.Add(tool.Key, new Dictionary<string, object>
                {
                    { "name", tool.Value.Name },
                    { "gameId", tool.Value.GameId },
                    { "access", tool.Value.AccessLevel }
                });
            }

            return JsonConvert.SerializeObject(new Dictionary<string, Dictionary<string, Dictionary<string, object>>>
            {
                { "games", gameList },
                { "tools", toolList }
            });
        }

        public string CompileForServer(GameManifest gameManifest, ToolManifest toolManifest)
        {
            return CompileForClient(gameManifest, toolManifest);
        }
    }
}
