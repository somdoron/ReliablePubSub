using System;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    static class PublisherExtensions
    {
        public static void Publish<TValue>(this Publisher publisher, ITopicConfig topicConfig, TValue value)
        {
            var config = topicConfig as TopicConfig<TValue>;
            if (config == null)
                throw new ApplicationException($"Config for topic {topicConfig.Topic} is not defined for type {typeof(TValue)}");

            var data = config.Serializer.Serialize(value);
            var key = config.KeyExtractor.Extract(value);
            publisher.Publish(topicConfig.Topic, key, data);
        }
    }
}