using NoDev.Infinity.User;

namespace NoDev.Infinity.Tools
{
    internal sealed class ToolInfo
    {
        internal string ID { get; private set; }
        internal string GameID { get; private set; }
        internal string Name { get; private set; }
        internal AccessLevel AccessLevel { get; private set; }

        internal ToolInfo(string id, string gameId, string name, AccessLevel access)
        {
            ID = id;
            GameID = gameId;
            Name = name;
            AccessLevel = access;
        }
    }
}
