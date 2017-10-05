using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ReliablePubSub.Common
{
    public class DefaultLastValueCache<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, ILastValueCache<TKey, TValue>
    {
        public void UpdateValue(TKey key, TValue value)
        {
            this[key] = value;
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> All()
        {
            return this;
        }
    }
}