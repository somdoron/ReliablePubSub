using System.Text;
using NetMQ;

namespace ReliablePubSub.Common
{
    public static class NetMqMessageExtensions
    {
        public static void Parse(this NetMQMessage message, out byte[] body, out string topic)
        {
            string address;
            Parse(message, out body, out topic, out address);
        }

        public static void Parse(this NetMQMessage message, out byte[] body, out string topic, out string address)
        {
            address = string.Empty;
            if (message.FrameCount > 2)
                address = message.Pop().ConvertToString();

            topic = string.Empty;
            if (message.FrameCount > 1)
                topic = Encoding.Unicode.GetString(message.Pop().ToByteArray());

            body = message.Pop().ToByteArray();
        }

        public static NetMQMessage CreateMessage(string topic, byte[] data)
        {
            var message = new NetMQMessage();
            if (!string.IsNullOrEmpty(topic)) message.Append(topic);
            message.Append(data);
            return message;
        }
    }
}
