using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;

namespace ReliablePubSub.Client
{
    class Program
    {
        private static void Main(string[] args)
        {
            //new Simple().Run("tcp://localhost:6669");
            var client = new ReliableClient(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5), m =>
             {
                 Console.WriteLine($"Message received. Topic: {m.First.ConvertToString()}, Message: {m.Last.ConvertToString()}");
             }, (x, m) => Console.WriteLine($"Error in message handler. Exception {x} Message {m}"),
             "tcp://localhost:6669");

            client.Subscribe("A");

            Console.ReadLine();
        }
    }
}
