
using System.Collections.Generic;

namespace NoDev.Common.Storage
{
    public interface ISettingsStorage : IEnumerable<SettingKeyValuePair>
    {
        object this[string key] { get; set; }

        bool ContainsKey(string key);

        bool Save();
    }

    public sealed class SettingKeyValuePair
    {
        public string Key { get; private set; }
        public object Value { get; private set; }

        public SettingKeyValuePair(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }
}
