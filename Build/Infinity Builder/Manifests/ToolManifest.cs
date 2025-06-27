using System.Collections.Generic;
using System.IO;
using NoDev.Infinity.Build.InfinityBuilder.Solutions;
using Newtonsoft.Json;

namespace NoDev.Infinity.Build.InfinityBuilder.Manifests
{
    internal class ToolManifest : Dictionary<string, ToolInfo>
    {
        internal string AddFromManifest(CSharpProject project, string manifestFile)
        {
            var i = JsonConvert.DeserializeObject<ToolInfo>(File.ReadAllText(manifestFile));

#if !DEBUG
            if (i.Ignore)
                return null;
#endif

            i.Project = project;
            Add(i.Id, i);

            return i.Id;
        }
    }
}
