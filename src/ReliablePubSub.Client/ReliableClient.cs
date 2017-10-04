using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace ReliablePubSub.Client
{
    public class ReliableClient : IDisposable
    {
        private const string SubscribeCommand = "S";
        private readonly TimeSpan _connectionTimeOut;
        private readonly TimeSpan _reconnectInterval;
        private const string WelcomeMessage = "WM";
        private const string HeartbeatMessage = "HB";

        private readonly string[] _addresses;
        private readonly Action<NetMQMessage> _subscriberMessageHandler;
        private readonly Action<Exception, NetMQMessage> _subscriberErrorHandler;

        private readonly NetMQActor _actor;
        private NetMQPoller _poller;
        private NetMQTimer _timeoutTimer;
        private NetMQTimer _reconnectTimer;
        private SubscriberSocket _subscriber;


        readonly List<string> _subscriptions = new List<string>();
        private PairSocket _shim;

        /// <summary>
        /// Create reliable client
        /// </summary>
        /// <param name="reconnectInterval"></param>
        /// <param name="subscriberMessageHandler"></param>
        /// <param name="subscriberErrorHandler"></param>
        /// <param name="addresses">addresses of the reliable servers</param>
        /// <param name="connectionTimeOut"></param>
        public ReliableClient(TimeSpan connectionTimeOut, TimeSpan reconnectInterval, Action<NetMQMessage> subscriberMessageHandler = null, Action<Exception, NetMQMessage> subscriberErrorHandler = null, params string[] addresses)
        {
            _connectionTimeOut = connectionTimeOut;
            _reconnectInterval = reconnectInterval;
            _addresses = addresses;
            _subscriberMessageHandler = subscriberMessageHandler;
            _subscriberErrorHandler = subscriberErrorHandler;
            _actor = NetMQActor.Create(Run);
        }

        private void Run(PairSocket shim)
        {
            _shim = shim;
            shim.ReceiveReady += OnShimMessage;

            _timeoutTimer = new NetMQTimer(_connectionTimeOut);
            _timeoutTimer.Elapsed += OnTimeoutTimer;

            _reconnectTimer = new NetMQTimer(_reconnectInterval);
            _reconnectTimer.Elapsed += OnReconnectTimer;

            _poller = new NetMQPoller { shim, _timeoutTimer, _reconnectTimer };

            shim.SignalOK();

            Connect();

            _poller.Run();

            _subscriber?.Dispose();
        }

        private void OnReconnectTimer(object sender, NetMQTimerEventArgs e)
        {
            // try to connect again
            Connect();
        }

        private void OnTimeoutTimer(object sender, NetMQTimerEventArgs e)
        {
            // dispose the current subscriber socket and try to connect
            _poller.Remove(_subscriber);
            _subscriber.Dispose();
            _subscriber = null;
            Connect();
        }

        private void OnShimMessage(object sender, NetMQSocketEventArgs e)
        {
            string command = e.Socket.ReceiveFrameString();

            if (command == NetMQActor.EndShimMessage)
            {
                _poller.Stop();
            }
            else if (command == SubscribeCommand)
            {
                string topic = e.Socket.ReceiveFrameString();
                _subscriptions.Add(topic);
                _subscriber?.Subscribe(topic);
            }
        }

        private void OnSubscriberMessage(object sender, NetMQSocketEventArgs e)
        {
            // we just forward the message to the actor
            var message = _subscriber.ReceiveMultipartMessage();

            var topic = message[0].ConvertToString();

            if (topic == WelcomeMessage)
            {
                // TODO: disconnection has happend, we might want to get snapshot from server
            }
            else if (topic == HeartbeatMessage)
            {
                // we got a heartbeat, lets postponed the timer
                _timeoutTimer.Enable = false;
                _timeoutTimer.Enable = true;
            }
            else
            {
                _shim.SendMultipartMessage(message);
            }
        }

        private void OnActorMessage(object sender, NetMQActorEventArgs e)
        {
            var message = e.Actor.ReceiveMultipartMessage();

            try
            {
                _subscriberMessageHandler?.Invoke(message);
            }
            catch (Exception ex)
            {
                _subscriberErrorHandler?.Invoke(ex, message);
            }
        }

        private void Connect()
        {
            var sockets = new List<SubscriberSocket>();
            var poller = new NetMQPoller();

            SubscriberSocket connectedSocket = null;

            // event handler to handle message from socket
            EventHandler<NetMQSocketEventArgs> handleMessage = (sender, args) =>
            {
                connectedSocket = (SubscriberSocket)args.Socket;
                poller.Stop();
            };

            var timeoutTimer = new NetMQTimer(_connectionTimeOut);

            // just cancel the poller without seting the connected socket
            timeoutTimer.Elapsed += (sender, args) => poller.Stop();
            poller.Add(timeoutTimer);

            foreach (var address in _addresses)
            {
                var socket = new SubscriberSocket();
                sockets.Add(socket);

                socket.ReceiveReady += handleMessage;
                poller.Add(socket);

                // Subscribe to welcome message
                socket.Subscribe(WelcomeMessage);
                socket.Connect(address);
            }

            poller.Run();

            // if we a connected socket the connection attempt succeed
            if (connectedSocket != null)
            {
                // remove the connected socket form the list
                sockets.Remove(connectedSocket);

                // close all exsiting connections
                foreach (var socket in sockets)
                {
                    // to close them immediatly we set the linger to zero
                    socket.Options.Linger = TimeSpan.Zero;
                    socket.Dispose();
                }

                // set the socket
                _subscriber = connectedSocket;

                // drop the welcome message
                _subscriber.SkipMultipartMessage();

                // subscribe to heartbeat
                _subscriber.Subscribe(HeartbeatMessage);

                // subscribe to all subscriptions
                foreach (string subscription in _subscriptions)
                {
                    _subscriber.Subscribe(subscription);
                }

                _subscriber.ReceiveReady -= handleMessage;
                _subscriber.ReceiveReady += OnSubscriberMessage;

                _actor.ReceiveReady += OnActorMessage;
                _poller.Add(_actor);

                _poller.Add(_subscriber);


                _timeoutTimer.Enable = true;
                _reconnectTimer.Enable = false;
            }
            else
            {
                // close all exsiting connections
                foreach (var socket in sockets)
                {
                    // to close them immediatly we set the linger to zero
                    socket.Options.Linger = TimeSpan.Zero;
                    socket.Dispose();
                }

                _reconnectTimer.Enable = true;
                _timeoutTimer.Enable = false;
            }
        }

        public void Subscribe(string topic)
        {
            _actor.SendMoreFrame(SubscribeCommand).SendFrame(topic);
        }

        public NetMQMessage ReceiveMessage()
        {
            return _actor.ReceiveMultipartMessage();
        }

        public void Dispose()
        {
            _actor.Dispose();
        }
    }
}
