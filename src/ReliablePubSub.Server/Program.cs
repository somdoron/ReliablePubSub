using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;

namespace ReliablePubSub.Server
{
    class Program
    {
        private static void Main(string[] args)
        {
            ReliableServer server = new ReliableServer("tcp://*:6669");

            while (true)
            {
                NetMQMessage message = new NetMQMessage();
                message.Append("A");
                message.Append(new Random().Next().ToString());
                server.Publish(message);

                Thread.Sleep(10);
            }
        }
    }
}
