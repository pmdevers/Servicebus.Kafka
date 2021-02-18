using System;
using System.Collections.Generic;
using System.Text;
using PMDEvers.Servicebus;

namespace Servicebus.Kafka.Test
{
    [KafkaTopic("TestTopic")]
    public class TestEvent : IEvent
    {
    }
}
