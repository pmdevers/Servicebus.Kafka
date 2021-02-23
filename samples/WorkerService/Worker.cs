using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WorkerService
{
   public class Worker : BackgroundService
   {
      private readonly ILogger<Worker> _logger;

      public Worker(ILogger<Worker> logger)
      {
         _logger = logger;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         await new Producer().Produce(stoppingToken);
      }
   }
   public class Worker1 : BackgroundService
   {
      private readonly ILogger<Worker1> _logger;

      public Worker1(ILogger<Worker1> logger)
      {
         _logger = logger;
      }

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
      {
         await new Producer1().Produce(stoppingToken);
      }
   }
}
