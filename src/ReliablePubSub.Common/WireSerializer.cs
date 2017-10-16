using System.IO;
using Wire;

namespace ReliablePubSub.Common
{
    public class WireSerializer : ISerializer
    {
        readonly Serializer _serializer = new Serializer(new SerializerOptions()); //TODO: consider to use known types

        public byte[] Serialize(object value)
        {
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(value, stream);
                return stream.ToArray();
            }
        }

        public object Deserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return _serializer.Deserialize(stream);
            }
        }
    }
}