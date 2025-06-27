#if DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NoDev.Infinity.Tools.Info;
using NoDev.Infinity.User;
using Newtonsoft.Json;

namespace NoDev.Infinity.Tools
{
    internal class DebugToolRetriever : IToolImageRetriever, IToolInfoRetriever
    {
        private static readonly object Lock = new object();

        private readonly string _toolsDir;
        private readonly string _manifestFileName;

        private Dictionary<string, ToolInfo> _toolInfos;
        private Dictionary<string, string> _toolImages;

        internal DebugToolRetriever(string toolsDir, string manifestFileName)
        {
            _toolsDir = toolsDir;
            _manifestFileName = manifestFileName;
        }

        public bool HasToolImage(string toolId)
        {
            if (_toolImages == null)
                LoadManifestsFromLocalSolution();

            Debug.Assert(_toolImages != null, "_toolImages != null");

            return _toolImages.ContainsKey(toolId);
        }

        public string RetrieveToolImageLocation(string toolId)
        {
            if (_toolImages == null)
                LoadManifestsFromLocalSolution();

            Debug.Assert(_toolImages != null, "_toolImages != null");

            return _toolImages.ContainsKey(toolId) ? Path.GetFullPath(_toolImages[toolId]) : null;
        }

        public ToolInfo RetrieveToolInfo(string toolId)
        {
            if (_toolInfos == null)
                LoadManifestsFromLocalSolution();

            Debug.Assert(_toolInfos != null, "_toolInfos != null");

            return _toolInfos.ContainsKey(toolId) ? _toolInfos[toolId] : null;
        }

        public IEnumerable<ToolInfo> RetrieveToolInfoByGameId(string gameId)
        {
            if (_toolInfos == null)
                LoadManifestsFromLocalSolution();

            return from toolInfo in _toolInfos where toolInfo.Value.GameID == gameId select toolInfo.Value;
        }

        private void LoadManifestsFromLocalSolution()
        {
            lock (Lock)
            {
                var projectDirs = Directory.GetDirectories(_toolsDir);

                _toolImages = new Dictionary<string, string>();
                _toolInfos = new Dictionary<string, ToolInfo>();

                foreach (var projectDir in projectDirs)
                    LoadManifestsFromLocalSolution(projectDir);
            }
        }

        private void LoadManifestsFromLocalSolution(string dir)
        {
            var manifestFiles = Directory.GetFiles(dir, _manifestFileName);

            if (manifestFiles.Length != 1)
            {
                var dirs = Directory.GetDirectories(dir);

                foreach (var childDir in dirs)
                    LoadManifestsFromLocalSolution(childDir);

                return;
            }

            var dbgBinDir = dir + @"\bin\Debug";

            if (!Directory.Exists(dbgBinDir))
                return;

            var toolDll = Directory.GetFiles(dbgBinDir, "*-*-*-*-*.dll");

            if (toolDll.Length != 1)
                return;

            var manifest = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(manifestFiles[0]));

            if (manifest.ContainsKey("ignore") && (bool)manifest["ignore"])
                return;

            if (!manifest.ContainsKey("id") || !manifest.ContainsKey("name") || !manifest.ContainsKey("game") || !manifest.ContainsKey("access"))
                throw new Exception("The tool manifest file must contain ID, name, game ID, and secure properties.");

            var toolId = (string)manifest["id"];

            _toolImages.Add(toolId, toolDll[0]);
            _toolInfos.Add(toolId, new ToolInfo(toolId, (string)manifest["game"], (string)manifest["name"], (AccessLevel)(long)manifest["access"]));
        }
    }
}
#endif