using System;
using NoDev.InfinityToolLib;

namespace NoDev.Infinity
{
    internal class InfinitySettings : DictionarySettingsStorage
    {
        internal event EventHandler<SettingChangedEventArgs> SettingChanged;

        internal InfinitySettings(string filename) : base(filename)
        {

        }

        public override object this[string key]
        {
            get { return base[key]; }
            set
            {
                base[key] = value;

                if (SettingChanged != null)
                    SettingChanged(this, new SettingChangedEventArgs(key, value));
            }
        }
    }

    internal static class InfinitySetting
    {
        internal const string
            WindowTitleMode = "windowTitleMode",
            CustomWindowTitleText = "windowTitleText"
            ;
    }

    internal sealed class SettingChangedEventArgs : EventArgs
    {
        internal string SettingName { get; private set; }
        internal object SettingValue { get; private set; }

        internal SettingChangedEventArgs(string name, object value)
        {
            SettingName = name;
            SettingValue = value;
        }
    }

    internal enum WindowTitleMode
    {
        Default,
        Randomize,
        Custom
    }
}
