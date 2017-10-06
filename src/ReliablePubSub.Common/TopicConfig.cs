using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliablePubSub.Common
{
    public class TopicConfig<TValue> : ITopicConfig
    {
        public TopicConfig(string topic)
        {
            Topic = topic;
        }

        public ILastValueCache<string, TValue> LastValueCache { get; set; }
        public IKeyExtractor<TValue, string> KeyExtractor { get; set; }
        public ISerializer<TValue> Serializer { get; set; }
        public IComparer<TValue> Comparer { get; set; }
        public string Topic { get; }
    }

    public interface ITopicConfig
    {
        string Topic { get; }
    }
}
