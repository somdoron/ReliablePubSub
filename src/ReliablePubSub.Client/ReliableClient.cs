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
        private string SubscribeCommand = "S";
        private readonly TimeSpan TimeOut = TimeSpan.FromSeconds(5);
        private readonly TimeSpan ReconnectTimer = TimeSpan.FromSeconds(5);
        private const string WelcomeMessage = "WM";
        private const string HeartbeatMessage = "HB";

        private readonly string[] m_addresses;
        private readonly Action<NetMQMessage> _subscriberMessageHandler;

        private NetMQActor m_actor;
        private NetMQPoller m_poller;
        private NetMQTimer m_timeoutTimer;
        private NetMQTimer m_reconnectTimer;
        private SubscriberSocket m_subscriber;


        List<string> m_subscriptions = new List<string>();
        private PairSocket m_shim;

        /// <summary>
        /// Create reliable client
        /// </summary>
        /// <param name="context"></param>
        /// <param name="addresses">addresses of the reliable servers</param>
        public ReliableClient(Action<NetMQMessage> subscriberMessageHandler = null, params string[] addresses)
        {
            m_addresses = addresses;
            _subscriberMessageHandler = subscriberMessageHandler;
            m_actor = NetMQActor.Create(Run);
        }

        private void Run(PairSocket shim)
        {
            m_shim = shim;
            shim.ReceiveReady += OnShimMessage;

            m_timeoutTimer = new NetMQTimer(TimeOut);
            m_timeoutTimer.Elapsed += OnTimeoutTimer;

            m_reconnectTimer = new NetMQTimer(ReconnectTimer);
            m_reconnectTimer.Elapsed += OnReconnectTimer;

            m_poller = new NetMQPoller { shim, m_timeoutTimer, m_reconnectTimer };

            shim.SignalOK();

            Connect();

            m_poller.Run();

            m_subscriber?.Dispose();
        }

        private void OnReconnectTimer(object sender, NetMQTimerEventArgs e)
        {
            // try to connect again
            Connect();
        }

        private void OnTimeoutTimer(object sender, NetMQTimerEventArgs e)
        {
            // dispose the current subscriber socket and try to connect
            m_poller.Remove(m_subscriber);
            m_subscriber.Dispose();
            m_subscriber = null;
            Connect();
        }

        private void OnShimMessage(object sender, NetMQSocketEventArgs e)
        {
            string command = e.Socket.ReceiveFrameString();

            if (command == NetMQActor.EndShimMessage)
            {
                m_poller.Stop();
            }
            else if (command == SubscribeCommand)
            {
                string topic = e.Socket.ReceiveFrameString();
                m_subscriptions.Add(topic);
                m_subscriber?.Subscribe(topic);
            }
        }

        private void OnSubscriberMessage(object sender, NetMQSocketEventArgs e)
        {
            // we just forward the message to the actor
            var message = m_subscriber.ReceiveMultipartMessage();

            var topic = message[0].ConvertToString();

            if (topic == WelcomeMessage)
            {
                // TODO: disconnection has happend, we might want to get snapshot from server
            }
            else if (topic == HeartbeatMessage)
            {
                // we got a heartbeat, lets postponed the timer
                m_timeoutTimer.Enable = false;
                m_timeoutTimer.Enable = true;
            }
            else
            {
                m_shim.SendMultipartMessage(message);
            }
        }

        private void OnActorMessage(object sender, NetMQActorEventArgs e)
        {
            _subscriberMessageHandler?.Invoke(e.Actor.ReceiveMultipartMessage());
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

            var timeoutTimer = new NetMQTimer(TimeOut);

            // just cancel the poller without seting the connected socket
            timeoutTimer.Elapsed += (sender, args) => poller.Stop();
            poller.Add(timeoutTimer);

            foreach (var address in m_addresses)
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
                m_subscriber = connectedSocket;

                // drop the welcome message
                m_subscriber.SkipMultipartMessage();

                // subscribe to heartbeat
                m_subscriber.Subscribe(HeartbeatMessage);

                // subscribe to all subscriptions
                foreach (string subscription in m_subscriptions)
                {
                    m_subscriber.Subscribe(subscription);
                }

                m_subscriber.ReceiveReady -= handleMessage;
                m_subscriber.ReceiveReady += OnSubscriberMessage;

                m_actor.ReceiveReady += OnActorMessage;
                m_poller.Add(m_actor);

                m_poller.Add(m_subscriber);


                m_timeoutTimer.Enable = true;
                m_reconnectTimer.Enable = false;
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

                m_reconnectTimer.Enable = true;
                m_timeoutTimer.Enable = false;
            }
        }

        public void Subscribe(string topic)
        {
            m_actor.SendMoreFrame(SubscribeCommand).SendFrame(topic);
        }

        public NetMQMessage ReceiveMessage()
        {
            return m_actor.ReceiveMultipartMessage();
        }

        public void Dispose()
        {
            m_actor.Dispose();
        }
    }
}
