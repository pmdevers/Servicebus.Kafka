using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using PMDEvers.Servicebus;
using WorkerService.Events;

namespace WorkerProducer
{
   public class Worker : BackgroundService
   {
      private readonly ILogger<Worker> _logger;
      private readonly IServiceProvider _serviceProvider;

      public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
      {
         _logger = logger;
         _serviceProvider = serviceProvider;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         while (!stoppingToken.IsCancellationRequested)
         {

            using (var scope = _serviceProvider.CreateScope())
            {
               var serviceBus = scope.ServiceProvider.GetService<IServiceBus>();

               await serviceBus.PublishAsync(new UserUpdated() {Name = "Test"}, stoppingToken);
            }

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
         }
      }
   }
}
