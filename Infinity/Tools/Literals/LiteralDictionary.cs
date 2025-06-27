using System.Collections.Concurrent;
using NoDev.InfinityToolLib.Tools;

namespace NoDev.Infinity.Tools.Literals
{
    internal class LiteralDictionary : ILiteralCollection
    {
        private readonly int _key;
        private readonly ConcurrentDictionary<int, object> _store;

        internal LiteralDictionary(int key)
        {
            _key = key;
            _store = new ConcurrentDictionary<int, object>();
        }

        internal void SetValue(int id, object value)
        {
            _store[id] = value;
        }

        public T GetValue<T>(int id)
        {
            return (T)_store[id ^ _key];
        }
    }
}
