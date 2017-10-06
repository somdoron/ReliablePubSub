using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    class SnapshotServer : IDisposable
    {
        private readonly string _address;
        private readonly ISnapshotGenerator _snapshotGenerator;
        private readonly NetMQActor _actor;
        private NetMQPoller _poller;

        public SnapshotServer(string address, ISnapshotGenerator snapshotGenerator)
        {
            if (snapshotGenerator == null) throw new ArgumentNullException(nameof(snapshotGenerator));

            _address = address;
            _snapshotGenerator = snapshotGenerator;
            _actor = NetMQActor.Create(Run);
        }

        private void Run(PairSocket shim)
        {
            using (var router = new RouterSocket())
            {
                router.Bind(_address);

                router.ReceiveReady += SendSnapshot;
                shim.ReceiveReady += OnShimMessage;

                // signal the actor that the shim is ready to work
                shim.SignalOK();

                _poller = new NetMQPoller { router, _actor };
                _poller.Run();
            }
        }

        private void OnShimMessage(object sender, NetMQSocketEventArgs e)
        {
            string command = e.Socket.ReceiveFrameString();
            if (command == NetMQActor.EndShimMessage)
            {
                // we got dispose command, we just stop the poller
                _poller.Stop();
            }
            else
                throw new NotImplementedException();
        }

        private void SendSnapshot(object sender, NetMQSocketEventArgs e)
        {
            var message = e.Socket.ReceiveMultipartMessage();
            string topic = message.Last.ConvertToString();

            var snapshot = _snapshotGenerator.GenerateSnapshot(topic);

            var response = new NetMQMessage(snapshot);
            response.PushEmptyFrame();
            response.Push(message.First);

            //TODO: consider sendmoreframe to stream the snapshot
            e.Socket.SendMultipartMessage(response);
        }

        public void Dispose()
        {
            _actor.Dispose();
        }
    }
}
