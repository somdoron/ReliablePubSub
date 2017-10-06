using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.ServiceModel.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    class Program
    {
        private static void Main(string[] args)
        {
            var topics = new Dictionary<string, ITopicConfig>();
            var topic1 = new TopicConfig<MyMessage>("topic1")
            {
                Serializer = new WireSerializer<MyMessage>(),
                Comparer = new DefaultComparer<MyMessage>(),
                KeyExtractor = new DefaultKeyExtractor<MyMessage, string>(x => x.Key)
            };
            topics.Add(topic1.Topic, topic1);

            var publisher = new Publisher("tcp://*:6669", "tcp://*:6668", topics.Keys);

            long id = 0;
            var rnd = new Random(1);
            while (true)
            {
                var message = new MyMessage()
                {
                    Id = id++,
                    Key = rnd.Next(1, 100).ToString(),
                    Body = $"Body: {Guid.NewGuid().ToString()}",
                    TimeStamp = DateTime.UtcNow
                };

                publisher.Publish(topics["topic1"], message);
                Thread.Sleep(100);
            }
        }
    }
}
