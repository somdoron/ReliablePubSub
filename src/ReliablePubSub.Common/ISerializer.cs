using Wire.Extensions;
using Wire.ValueSerializers;

namespace ReliablePubSub.Common
{
    public interface ISerializer
    {
        byte[] Serialize(object value);
        object Deserialize(byte[] bytes);
    }
}