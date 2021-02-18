using System;

namespace Servicebus.Kafka
{
    public class KafkaTopic : Attribute
    {
        public KafkaTopic(string topicName)
        {
            TopicName = topicName;
        }
        public string TopicName { get; }
    }
}
