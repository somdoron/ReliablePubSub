using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ReliablePubSub.Common
{
    public class DefaultLastValueCache<TValue> : ILastValueCache
    {
        private readonly Dictionary<string, ConcurrentDictionary<string, TValue>> _cache = new Dictionary<string, ConcurrentDictionary<string, TValue>>();
        public DefaultLastValueCache(IEnumerable<string> topics)
        {
            foreach (var topic in topics)
                _cache.Add(topic, new ConcurrentDictionary<string, TValue>());
        }
        public IEnumerable<TValue> All(string topic)
        {
            return _cache[topic].Values;
        }

        public bool TryGet(string topic, string key, out object data)
        {
            ConcurrentDictionary<string, TValue> cache;
            TValue item = default(TValue);
            bool result = _cache.TryGetValue(topic, out cache) && cache.TryGetValue(key, out item);
            data = item;
            return result;
        }

        public void AddOrUpdate(string topic, string key, object data)
        {
            ConcurrentDictionary<string, TValue> cache;
            if (_cache.TryGetValue(topic, out cache))
                cache[key] = (TValue)data;
        }

        public void Clear(string topic)
        {
            ConcurrentDictionary<string, TValue> cache;
            if (_cache.TryGetValue(topic, out cache))
                cache.Clear();
        }
    }
}