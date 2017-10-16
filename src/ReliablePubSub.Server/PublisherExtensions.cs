using System;
using System.Collections.Generic;
using ReliablePubSub.Common;

namespace ReliablePubSub.Server
{
    static class PublisherExtensions
    {
        public static void Publish<TValue>(this Publisher publisher, IDictionary<Type, TypeConfig> typeConfigs, string topic, TValue value)
        {
            TypeConfig config;
            if (!typeConfigs.TryGetValue(typeof(TValue), out config))
                throw new ApplicationException($"Config is not defined for type {typeof(TValue)}");

            var data = config.Serializer.Serialize(value);
            var key = config.KeyExtractor.Extract(value);
            publisher.Publish(topic, key, data);
        }
    }
}