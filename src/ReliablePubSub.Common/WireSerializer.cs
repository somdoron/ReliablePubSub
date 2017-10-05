using System.IO;
using Wire;

namespace ReliablePubSub.Common
{
    public class WireSerializer<TValue> : ISerializer<TValue>
    {
        readonly Serializer _serializer = new Serializer();

        public byte[] Serialize(TValue value)
        {
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(value, stream);
                return stream.ToArray();
            }
        }

        public TValue Deserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return _serializer.Deserialize<TValue>(stream);
            }
        }
    }
}