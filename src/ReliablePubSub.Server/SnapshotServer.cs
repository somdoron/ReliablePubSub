using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace ReliablePubSub.Server
{
    class SnapshotServer : IDisposable
    {
        private readonly string _address;
        private readonly NetMQActor _actor;
        private NetMQPoller _poller;
        //private RouterSocket _router;

        public SnapshotServer(string address)
        {
            _address = address;
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
            var snapshot = new byte[] { 1, 2, 3, 4, 5 }; //_snapshotFactory(topic);

            var response = new NetMQMessage();
            response.Append(message.First);
            response.AppendEmptyFrame();
            response.Append(snapshot);

            //TODO: consider sendmoreframe to stream the snapshot
            e.Socket.SendMultipartMessage(response);
        }

        public void Dispose()
        {
            _actor.Dispose();
        }
    }
}
