using System.Drawing;

namespace NoDev.Infinity.Tools
{
    internal sealed class GameInfo
    {
        internal string ID { get; private set; }
        internal string Name { get; private set; }
        internal Image Thumbnail { get; private set; }

        internal GameInfo(string id, string name, Image thumbnail)
        {
            ID = id;
            Name = name;
            Thumbnail = thumbnail;
        }
    }
}
