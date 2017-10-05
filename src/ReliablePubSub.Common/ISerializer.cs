using Wire.Extensions;
using Wire.ValueSerializers;

namespace ReliablePubSub.Common
{
    public interface ISerializer<TValue>
    {
        byte[] Serialize(TValue value);
        TValue Deserialize(byte[] bytes);
    }
}