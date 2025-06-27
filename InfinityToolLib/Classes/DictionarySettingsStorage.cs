using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NoDev.Common.Storage;
using Newtonsoft.Json;

namespace NoDev.InfinityToolLib
{
    public class DictionarySettingsStorage : ISettingsStorage
    {
        private readonly string _filePath;
        private IDictionary<string, object> _settings;

        public DictionarySettingsStorage(string filename)
        {
            _filePath = filename;
        }

        public virtual object this[string key]
        {
            get
            {
                EnsureOpened();
                return _settings.ContainsKey(key) ? _settings[key] : null;
            }
            set
            {
                EnsureOpened();
                _settings[key] = value;
            }
        }

        private void EnsureOpened()
        {
            if (_settings != null)
                return;

            if (!File.Exists(_filePath))
                _settings = new ConcurrentDictionary<string, object>();
            else
            {
                try
                {
                    _settings = JsonConvert.DeserializeObject<ConcurrentDictionary<string, object>>(File.ReadAllText(_filePath));
                }
                catch (Exception)
                {
                    _settings = new ConcurrentDictionary<string, object>();
                }
            }
        }

        public virtual bool ContainsKey(string key)
        {
            EnsureOpened();
            return _settings.ContainsKey(key);
        }

        public virtual bool Save()
        {
            if (_settings == null)
                return true;

            try
            {
                File.WriteAllText(_filePath, JsonConvert.SerializeObject(_settings, Formatting.Indented));

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerator<SettingKeyValuePair> GetEnumerator()
        {
            EnsureOpened();
            return _settings.Select(s => new SettingKeyValuePair(s.Key, s.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
