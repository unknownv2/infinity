using System.Collections.Generic;
using System.Linq;

namespace NoDev.Infinity.Tools.Info
{
    internal class ToolInfoRetriever : IToolInfoRetriever
    {
        internal List<IToolInfoRetriever> Retrievers { get; private set; }

        internal ToolInfoRetriever()
        {
            Retrievers = new List<IToolInfoRetriever>();
        }

        public IEnumerable<ToolInfo> RetrieveToolInfoByGameId(string gameId)
        {
            return Retrievers.SelectMany(f => f.RetrieveToolInfoByGameId(gameId));
        }

        public ToolInfo RetrieveToolInfo(string toolId)
        {
            return Retrievers.Select(f => f.RetrieveToolInfo(toolId)).FirstOrDefault(toolInfo => toolInfo != null);
        }
    }
}
