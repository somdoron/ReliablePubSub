using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    class Program
    {
        private static void Main(string[] args)
        {
            var publisher = new Publisher("tcp://*:6669", "tcp://*:6668");
            //var msg = new MyMessage() { Body = "asasas", Id = 2, Key = "AB", TimeStamp = DateTime.UtcNow };
            //var ser = new WireSerializer<MyMessage>();
            //var bytes = ser.Serialize(msg);
            //var msg2 = ser.Deserialize(bytes);

            //using (var server = new ReliableServer(TimeSpan.FromSeconds(5), "tcp://*:6669"))
            //using (var snapshotServer = new SnapshotServer("tcp://*:6668"))
            //{
            //    long id = 0;
            //    while (true)
            //    {

            //        NetMQMessage message = new NetMQMessage();
            //        message.Append("A");
            //        message.Append(new Random().Next().ToString());
            //        server.Publish(message);

            //        Thread.Sleep(100);
            //    }
            //}
        }
    }

    class Publisher : IDisposable
    {
        private readonly ReliableServer _publishServer;
        private readonly SnapshotServer _snapshotServer;
        private readonly ConcurrentDictionary<string, ITopicConfig> _topics = new ConcurrentDictionary<string, ITopicConfig>();

        public Publisher(string publisherAddress, string snapshotAddress)
        {
            _publishServer = new ReliableServer(TimeSpan.FromSeconds(5), publisherAddress);
            _snapshotServer = new SnapshotServer(snapshotAddress, GenerateSnapshot);
        }

        private ITopicConfig GetTopicConfig(string topic)
        {
            ITopicConfig configDef;
            if (!_topics.TryGetValue(topic, out configDef))
                throw new ApplicationException($"Topic {topic} is not registered. Please use RegisterTopic method with relevant config");

            return configDef;
        }

        private IEnumerable<byte[]> GenerateSnapshot(string topic)
        {
            var configDef = GetTopicConfig(topic);
            return configDef.GetSnapshot();
        }

        public bool TryRegisterTopic(ITopicConfig config)
        {
            return _topics.TryAdd(config.Topic, config);
        }

        public bool TryUnregisterTopic(string topic)
        {
            ITopicConfig config;
            return _topics.TryRemove(topic, out config);
        }

        public void Publish<TValue>(string topic, TValue value)
        {
            var configDef = GetTopicConfig(topic);

            var config = configDef as TopicConfig<TValue>;
            if (config == null)
                throw new ApplicationException($"Topic {topic} not registered for type {typeof(TValue).FullName}");

            var data = config.Serializer.Serialize(value);
            var message = NetMqMessageExtensions.CreateMessage(topic, data);
            //update LVC before publish, so its available for snapshots
            config.LastValueCache.UpdateValue(config.KeyExtractor.Extract(value), value);
            _publishServer.Publish(message);
        }

        public void Dispose()
        {
            _publishServer?.Dispose();
            _snapshotServer?.Dispose();
        }
    }
}
