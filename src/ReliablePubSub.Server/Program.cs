using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    class Program
    {
        private static void Main(string[] args)
        {
            //do
            //{
            //    Console.WriteLine("Running");
            //    using (var server = new ReliableServer(TimeSpan.FromSeconds(2), "tcp://*:6669"))
            //    {
            //        long id = 0;
            //        for (int i = 0; i < 100; i++)
            //        {
            //            NetMQMessage message = new NetMQMessage();
            //            message.Append("topic1");
            //            message.Append(DateTime.UtcNow.ToString());
            //            server.Publish(message);

            //            Thread.Sleep(100);
            //        }
            //    }
            //    Console.WriteLine("Stopped");
            //} while (Console.ReadKey().Key != ConsoleKey.Escape);

            var knownTypes = new Dictionary<Type, TypeConfig>();
            knownTypes.Add(typeof(MyMessage), new TypeConfig
            {
                Serializer = new WireSerializer(),
                Comparer = new DefaultComparer<MyMessage>(),
                KeyExtractor = new DefaultKeyExtractor<MyMessage>(x => x.Key)
            });

            var topics = new Dictionary<string, Type>();
            topics.Add("topic1", typeof(MyMessage));


            using (var publisher = new Publisher("tcp://*", 6669, 6668, topics.Keys))
            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
            {
                var task = Task.Run(() =>
                {

                    long id = 0;
                    var rnd = new Random(1);
                    while (!tokenSource.IsCancellationRequested)
                    {
                        var message = new MyMessage()
                        {
                            Id = id++,
                            Key = rnd.Next(1, 100).ToString(),
                            Body = $"Body: {Guid.NewGuid().ToString()}",
                            TimeStamp = DateTime.UtcNow
                        };
                        publisher.Publish(knownTypes, "topic1", message);
                        Thread.Sleep(100);
                    }
                }, tokenSource.Token);

                while (Console.ReadKey().Key != ConsoleKey.Escape) { }
                tokenSource.Cancel();
            }
        }
    }
}