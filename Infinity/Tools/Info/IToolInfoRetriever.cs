using System.Collections.Generic;

namespace NoDev.Infinity.Tools.Info
{
    internal interface IToolInfoRetriever
    {
        ToolInfo RetrieveToolInfo(string toolId);

        IEnumerable<ToolInfo> RetrieveToolInfoByGameId(string gameId);
    }
}
