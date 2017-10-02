using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace ReliablePubSub.Server
{
    class Simple
    {
        public static void Run()
        {
            using (var publisherSocket = new XPublisherSocket())
            {
                publisherSocket.SetWelcomeMessage("WM");
                publisherSocket.Bind("tcp://*:6669");

                // we just drop subscriptions                     
                publisherSocket.ReceiveReady += (sender, eventArgs) => publisherSocket.SkipMultipartMessage();

                var poller = new NetMQPoller { publisherSocket };

                // send a message every second
                var sendMessageTimer = new NetMQTimer(1000);
                poller.Add(sendMessageTimer);
                sendMessageTimer.Elapsed +=
                    (sender, eventArgs) =>
                        publisherSocket.SendMoreFrame("A").SendFrame(new Random().Next().ToString());

                // send heartbeat every two seconds
                var heartbeatTimer = new NetMQTimer(2000);
                poller.Add(heartbeatTimer);
                heartbeatTimer.Elapsed += (sender, eventArgs) => publisherSocket.SendFrame("HB");

                poller.Stop();
            }
        }
    }
}
