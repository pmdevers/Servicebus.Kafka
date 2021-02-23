using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using PMDEvers.Servicebus;
using WorkerService.Events;

namespace WorkerService
{
   public class Worker : BackgroundService
   {
      private readonly ILogger<Worker> _logger;
      private readonly IServiceProvider _serviceProvider;
      private readonly IServiceBus _serviceBus;

      public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
      {
         _logger = logger;
         _serviceProvider = serviceProvider;
         
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         using (var scope = _serviceProvider.CreateScope())
         {
            var serviceBus = scope.ServiceProvider.GetService<IServiceBus>();
            
            await serviceBus.PublishAsync(new UserUpdated() { Name = "Test" }, stoppingToken);
         }

         
      }
   }
   
}
