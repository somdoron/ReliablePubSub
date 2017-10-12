using System;
using System.Collections.Generic;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    class Publisher : IDisposable
    {

        private readonly ReliableServer _publishServer;
        private readonly SnapshotServer _snapshotServer;
        private readonly SnapshotCache _snapshotCache;

        public Publisher(string baseAddress, ushort publisherPort, ushort snapshotPort, IEnumerable<string> topics)
        {
            _snapshotCache = new SnapshotCache(topics);
            _publishServer = new ReliableServer(TimeSpan.FromSeconds(5), $"{baseAddress}:{publisherPort}");
            _snapshotServer = new SnapshotServer($"{baseAddress}:{snapshotPort}", _snapshotCache);
        }

        public void Publish(string topic, string key, byte[] data)
        {
            _snapshotCache.AddOrUpdate(topic, key, data);
            var message = NetMqMessageExtensions.CreateMessage(topic, data);
            _publishServer.Publish(message);
        }

        public void Dispose()
        {
            _publishServer?.Dispose();
            _snapshotServer?.Dispose();
        }
    }
}