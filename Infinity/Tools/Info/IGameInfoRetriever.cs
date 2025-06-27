using System.Collections.Generic;

namespace NoDev.Infinity.Tools
{
    internal interface IGameInfoRetriever
    {
        GameInfo RetrieveGameInfo(string gameId);

        IEnumerable<GameInfo> RetrieveGameInfo();
    }
}
