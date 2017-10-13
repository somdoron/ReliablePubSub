using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NetMQ;
using ReliablePubSub.Common;

namespace ReliablePubSub.Client
{
    public class Subscriber : IDisposable
    {
        private readonly ushort _publisherPort;
        private readonly ushort _snapshotPort;
        private readonly ReliableClient _client;
        private readonly IDictionary<Type, TypeConfig> _knownTypes;
        private readonly IDictionary<string, Type> _topics;
        private readonly ILastValueCache _lastValueCache;

        public Subscriber(IEnumerable<string> baseAddress, ushort publisherPort, ushort snapshotPort,
            IDictionary<Type, TypeConfig> knownTypes, IDictionary<string, Type> topics, ILastValueCache lastValueCache)
        {
            _knownTypes = knownTypes;
            _topics = topics;
            _lastValueCache = lastValueCache;
            _publisherPort = publisherPort;
            _snapshotPort = snapshotPort;

            _client = new ReliableClient(baseAddress.Select(x => $"{x}:{publisherPort}"), TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(5), HandleUpdate, (x, m) => Debug.WriteLine($"Error in message handler. Exception {x} Message {m}"),
                GetSnapshots);

            foreach (var topic in _topics.Keys)
            {
                _client.Subscribe(topic);
            }
        }

        private void HandleUpdate(NetMQMessage m)
        {
            var topic = m.First.ConvertToString();
            var message = m.Last.Buffer;
            var type = _knownTypes[_topics[topic]];
            var obj = type.Serializer.Deserialize(message);
            var key = type.KeyExtractor.Extract(obj);

            _lastValueCache.AddOrUpdate(topic, key, obj);

            Debug.WriteLine($"From subscriber:{obj}");
        }

        private void GetSnapshots()
        {
            var snapshotAddress =
                _client.SubscriberAddress.Replace(_publisherPort.ToString(), _snapshotPort.ToString());

            using (var snapshotClient = new SnapshotClient(TimeSpan.FromSeconds(30), snapshotAddress))
            {
                snapshotClient.Connect();
                foreach (var topic in _topics.Keys)
                {
                    NetMQMessage snapshot;
                    if (snapshotClient.TryGetSnapshot(topic, out snapshot))
                    {
                        foreach (var frame in snapshot)
                        {
                            var message = frame.Buffer;
                            var type = _knownTypes[_topics[topic]];
                            var obj = type.Serializer.Deserialize(message);
                            var key = type.KeyExtractor.Extract(obj);

                            object cachedValue;
                            if (!_lastValueCache.TryGet(topic, key, out cachedValue) || type.Comparer.Compare(cachedValue, obj) < 0)
                            {
                                _lastValueCache.AddOrUpdate(topic, key, obj);
                                Debug.WriteLine($"From snapshot:{obj}");
                            }
                            else
                                Debug.WriteLine($"Object from snapshot dropped. Cached: {cachedValue} Snapshot: {obj}");
                        }
                    }
                }
            }
        }


        public void Dispose()
        {
            _client.Dispose();
        }
    }
}