using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace ReliablePubSub.Client
{
    class SnapshotClient : IDisposable
    {
        private readonly string _address;
        private RequestSocket _socket;
        private readonly TimeSpan _timeout;

        public SnapshotClient(TimeSpan timeout, string address)
        {
            _address = address;
            _timeout = timeout;
        }

        public void Connect()
        {
            _socket = new RequestSocket();
            _socket.Connect(_address);
        }

        public bool TryGetSnapshot(string topic, out NetMQMessage snapshot)
        {
            _socket.SendFrame(topic);
            snapshot = new NetMQMessage();
            return _socket.TryReceiveMultipartMessage(_timeout, ref snapshot);
        }

        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}
