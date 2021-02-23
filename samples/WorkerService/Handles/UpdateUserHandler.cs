using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using PMDEvers.Servicebus;
using WorkerService.Events;

namespace WorkerService.Handles
{
   public class UpdateUserHandler : IEventHandler<UserUpdated>
   {
      public void Handle(UserUpdated @event)
      {
         Console.WriteLine("UpdateUser: " + @event.Name);
      }
   }
}
