namespace ReliablePubSub.Common
{
    public interface ILastValueCache
    {
        bool TryGet(string topic, string key, out object data);
        void AddOrUpdate(string topic, string key, object data);
        void Clear(string topic);
    }
}