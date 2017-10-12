using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePubSub.Common
{
    public class MyMessage : IComparable<MyMessage>
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public string Body { get; set; }
        public DateTime TimeStamp { get; set; }

        public int CompareTo(MyMessage other)
        {
            var keyComparison = string.Compare(Key, other.Key, StringComparison.Ordinal);
            if (keyComparison != 0) return keyComparison;
            var idComparison = Id.CompareTo(other.Id);
            return idComparison;
        }

        public override string ToString()
        {
            return $"Id:{Id} Key:{Key} Body:{Body} Time:{TimeStamp:hh:mm:ss.fff}";
        }
    }
}
