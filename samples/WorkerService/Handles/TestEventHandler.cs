using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using PMDEvers.Servicebus;
using WorkerService.Events;

namespace WorkerService.Handles
{
   public class TestEventHandler : IEventHandler<TestEvent>
   {
      public void Handle(TestEvent @event)
      {
         Debug.WriteLine($"Message Received: {@event.Name}");
      }
   }
}
