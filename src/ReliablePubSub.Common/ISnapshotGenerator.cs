using System.Collections.Generic;

namespace ReliablePubSub.Common
{
    public interface ISnapshotGenerator
    {
        IEnumerable<byte[]> GenerateSnapshot(string topic);
    }
}
