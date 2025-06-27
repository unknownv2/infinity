using System.Windows.Forms;
using NoDev.Common.Storage;

namespace NoDev.InfinityToolLib.Tools
{
    public interface ITool
    {
        void SetSettings(ISettingsStorage settings);

        Control Panel { get; }
    }
}
