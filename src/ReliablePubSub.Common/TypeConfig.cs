using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePubSub.Common
{
    public class TypeConfig
    {
        public TypeConfig(Type type)
        {
            Type = type;
        }
        public IKeyExtractor KeyExtractor { get; set; }
        public ISerializer Serializer { get; set; }
        public IComparer Comparer { get; set; }
        public ILastValueCache LastValueCache { get; set; }
        Type Type { get; }
    }
}
