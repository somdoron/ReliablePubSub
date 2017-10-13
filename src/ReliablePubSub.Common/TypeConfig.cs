using System.Collections;

namespace ReliablePubSub.Common
{
    public class TypeConfig
    {
        public IKeyExtractor KeyExtractor { get; set; }
        public ISerializer Serializer { get; set; }
        public IComparer Comparer { get; set; }
        public ILastValueCache LastValueCache { get; set; }
    }
}
