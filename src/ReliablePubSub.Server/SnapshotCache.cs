using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    public class SnapshotCache : LastValueCache<string, byte[]>, ISnapshotGenerator
    {
        public SnapshotCache(IEnumerable<string> topics) : base(topics)
        {
        }

        public IEnumerable<byte[]> GenerateSnapshot(string topic)
        {
            return All(topic);
        }
    }
}