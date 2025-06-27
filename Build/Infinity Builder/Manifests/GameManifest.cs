using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NoDev.Infinity.Build.InfinityBuilder.Manifests
{
    internal class GameManifest : Dictionary<string, GameInfo>
    {
        internal GameManifest()
        {
            
        }

        internal GameManifest(string manifestFile)
        {
            JsonConvert.PopulateObject(File.ReadAllText(manifestFile), this);
        }
    }
}
