using System.Collections.Generic;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    public class SnapshotCache : DefaultLastValueCache<byte[]>, ISnapshotGenerator
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