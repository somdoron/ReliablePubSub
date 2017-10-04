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
            using (var snapshotServer = new SnapshotServer("tcp://*:6668"))
            {

            }
            Console.ReadLine();

            using (var server = new ReliableServer(TimeSpan.FromSeconds(5), "tcp://*:6669"))
            {
                while (true)
                {
                    NetMQMessage message = new NetMQMessage();
                    message.Append("A");
                    message.Append(new Random().Next().ToString());
                    server.Publish(message);

                    Thread.Sleep(100);
                }
            }
        }
    }
}
