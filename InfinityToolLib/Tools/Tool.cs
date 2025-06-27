using System;
using System.ComponentModel;
using System.Windows.Forms;
using NoDev.Common.Storage;
using NoDev.InfinityToolLib.Panels;

namespace NoDev.InfinityToolLib.Tools
{
    [ToolboxItem(false)]
    public class Tool : SideMenuInfinityPanel, ITool
    {
        protected ISettingsStorage Settings { get; private set; }

        public void SetSettings(ISettingsStorage settings)
        {
            if (Settings != null)
                throw new Exception("Cannot set settings more than one time.");

            Settings = settings;
        }

        public Control Panel
        {
            get { return this; }
        }

        protected virtual void SetEnabledState(bool enabled)
        {
            ScrollPanel.Enabled = enabled;
        }
    }
}
