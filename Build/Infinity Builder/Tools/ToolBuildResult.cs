using System.Collections.Generic;

namespace NoDev.Infinity.Build.InfinityBuilder.Tools
{
    internal class ToolBuildResult
    {
        internal IDictionary<int, object> ReplacedLiterals { get; set; }
        internal string OutputAssemblyPath { get; set; }

        internal bool Success
        {
            get { return OutputAssemblyPath != null; }
        }
    }
}
