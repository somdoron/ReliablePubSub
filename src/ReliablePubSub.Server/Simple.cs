using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace ReliablePubSub.Server
{
    class Simple
    {
        public static void Run()
        {
            using (NetMQContext context = NetMQContext.Create())
            {
                using (var publisherSocket = context.CreateXPublisherSocket())
                {
                    publisherSocket.SetWelcomeMessage("WM");
                    publisherSocket.Bind("tcp://*:6669");

                    // we just drop subscriptions                     
                    publisherSocket.ReceiveReady += (sender, eventArgs) => publisherSocket.SkipMultipartMessage();

                    Poller poller = new Poller(publisherSocket);

                    // send a message every second
                    NetMQTimer sendMessageTimer = new NetMQTimer(1000);
                    poller.AddTimer(sendMessageTimer);
                    sendMessageTimer.Elapsed +=
                        (sender, eventArgs) =>
                            publisherSocket.SendMoreFrame("A").SendFrame(new Random().Next().ToString());

                    // send heartbeat every two seconds
                    NetMQTimer heartbeatTimer = new NetMQTimer(2000);
                    poller.AddTimer(heartbeatTimer);
                    heartbeatTimer.Elapsed += (sender, eventArgs) => publisherSocket.SendFrame("HB");

                    poller.PollTillCancelled();
                }
            }
        }
    }
}
