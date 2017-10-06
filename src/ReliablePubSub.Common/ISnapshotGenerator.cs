using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePubSub.Common
{
    public interface ISnapshotGenerator
    {
        IEnumerable<byte[]> GenerateSnapshot(string topic);
    }
}
