using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using PMDEvers.Servicebus;
using Servicebus.Kafka;
using WorkerService.Events;
using WorkerService.Handles;

namespace WorkerService
{
   public class Program
   {
      public static void Main(string[] args)
      {
         CreateHostBuilder(args).Build().Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .ConfigureServices((hostContext, services) =>
              {
                 services.AddHostedService<Worker>();

                 services.AddServiceBus()
                    .AddEventHandler<TestEvent, TestEventHandler>()
                    .AddEventHandler<UserUpdated, UpdateUserHandler>()
                    .ProduceEvent<UserUpdated>();

                  
                 services.AddServiceBusKafka(
                    new ConsumerConfig()
                    {
                       BootstrapServers = "10.107.126.142:9094", 
                       GroupId = typeof(Program).Assembly.GetName().Name,
                       AutoOffsetReset = AutoOffsetReset.Earliest,
                       Acks = Acks.Leader,
                       EnableAutoCommit = false,
                       MaxPollIntervalMs = (int)TimeSpan.FromMinutes(10).TotalMilliseconds,
                       SessionTimeoutMs = (int)TimeSpan.FromSeconds(10).TotalMilliseconds,
                       EnablePartitionEof = true,
                    },
                    new SchemaRegistryConfig() {Url = "http://10.101.16.135:8081" });
              });
   }
}
