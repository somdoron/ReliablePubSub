namespace ReliablePubSub.Common
{
    public interface ISerializer
    {
        byte[] Serialize(object value);
        object Deserialize(byte[] bytes);
    }
}