﻿using System;
using System.Collections.Generic;
using System.Text;
using PMDEvers.Servicebus;

namespace Servicebus.Kafka.Test
{
    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public void Handle(TestEvent @event)
        {
            
        }
    }
}
