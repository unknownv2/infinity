using System.Collections.Generic;
using System.Linq;

namespace NoDev.Infinity.Tools
{
    internal class ToolImageRetriever : IToolImageRetriever
    {
        internal List<IToolImageRetriever> Retrievers { get; private set; }

        internal ToolImageRetriever()
        {
            Retrievers = new List<IToolImageRetriever>();
        }

        public bool HasToolImage(string toolId)
        {
            return Retrievers.Any(r => r.HasToolImage(toolId));
        }

        public string RetrieveToolImageLocation(string toolId)
        {
            return Retrievers.Select(r => r.RetrieveToolImageLocation(toolId)).FirstOrDefault(e => e != null);
        }
    }
}
