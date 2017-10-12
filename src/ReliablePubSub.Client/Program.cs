using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ReliablePubSub.Common;

namespace ReliablePubSub.Client
{
    class Program
    {
        private static void Main(string[] args)
        {
            var knownTypes = new Dictionary<Type, TypeConfig>();
            knownTypes.Add(typeof(MyMessage), new TypeConfig(typeof(MyMessage))
            {
                Serializer = new WireSerializer(),
                Comparer = new DefaultComparer<MyMessage>(),
                KeyExtractor = new DefaultKeyExtractor<MyMessage>(x => x.Key)
            });

            var topics = new Dictionary<string, Type>();
            topics.Add("topic1", typeof(MyMessage));

            var subscriber = new Subscriber(new[] { "tcp://localhost" }, 6669, 6668, knownTypes, topics, new DefaultLastValueCache<object>(topics.Keys));


            //using (var snapshotClient = new SnapshotClient(TimeSpan.FromSeconds(30), "tcp://localhost:6668"))
            //{
            //    snapshotClient.Connect();
            //    NetMQMessage snapshot;
            //    if (snapshotClient.TryGetSnapshot("A", out snapshot))
            //        Console.WriteLine(snapshot.Last.ConvertToString());
            //}

            //new Simple().Run("tcp://localhost:6669");
            //var client = new ReliableClient(new[] { "tcp://localhost:6669" }, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), m =>
            //  {
            //      Console.WriteLine($"Message received. Topic: {m.First.ConvertToString()}, Message: {m.Last.ConvertToString()}");
            //  }, (x, m) => Console.WriteLine($"Error in message handler. Exception {x} Message {m}"),
            //    null);

            //client.Subscribe("topic1");

            Console.ReadLine();
        }
    }
}
