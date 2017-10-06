using System.Collections.Generic;

namespace ReliablePubSub.Common
{
    public interface ILastValueCache<in TKey, TValue>
    {
        IEnumerable<TValue> All(string topic);
        void AddOrUpdate(string topic, TKey key, TValue data);
        void Clear();
    }
}