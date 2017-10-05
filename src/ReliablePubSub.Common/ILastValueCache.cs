using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePubSub.Common
{
    public interface ILastValueCache<TKey, TValue>
    {
        TValue this[TKey key] { get; set; }
        bool TryGetValue(TKey key, out TValue value);
        void UpdateValue(TKey key, TValue value);
        IEnumerable<KeyValuePair<TKey, TValue>> All();
    }
}
