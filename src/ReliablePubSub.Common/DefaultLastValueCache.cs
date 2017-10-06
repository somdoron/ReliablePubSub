using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ReliablePubSub.Common
{
    public class LastValueCache<TKey, TValue> : ILastValueCache<TKey, TValue>
    {
        private readonly Dictionary<string, ConcurrentDictionary<TKey, TValue>> _cache = new Dictionary<string, ConcurrentDictionary<TKey, TValue>>();
        public LastValueCache(IEnumerable<string> topics)
        {
            foreach (var topic in topics)
                _cache.Add(topic, new ConcurrentDictionary<TKey, TValue>());
        }
        public IEnumerable<TValue> All(string topic)
        {
            return _cache[topic].Values;
        }
        public void AddOrUpdate(string topic, TKey key, TValue data)
        {
            _cache[topic][key] = data;
        }
        public void Clear()
        {
            foreach (var kv in _cache)
                kv.Value.Clear();
        }
    }
}