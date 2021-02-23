using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using WorkerService.Events;

namespace WorkerService
{
   public class Producer1
   {
      public async Task Produce(CancellationToken cancellationToken)
      {
         var producerConfig = new ProducerConfig
         {
            BootstrapServers = "10.107.126.142:9094"
         };

         var schemaRegistryConfig = new SchemaRegistryConfig
         {
            // Note: you can specify more than one schema registry url using the
            // schema.registry.url property for redundancy (comma separated list). 
            // The property name is not plural to follow the convention set by
            // the Java implementation.
            Url = "http://10.101.16.135:8081"
         };

         using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var producer =
                new ProducerBuilder<int, UserUpdated>(producerConfig)
                    .SetKeySerializer(new AvroSerializer<int>(schemaRegistry))
                    .SetValueSerializer(new AvroSerializer<UserUpdated>(schemaRegistry))
                    .Build())
            {
                int i = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                   i++;
                   var user = new UserUpdated() { Name = $"Message {i}" };

                   //var result = await new AvroSerializer<TestEvent>(schemaRegistry).SerializeAsync(user, new SerializationContext());

                    //await schemaRegistry.RegisterSchemaAsync("test", user.Schema);
                    await producer
                        .ProduceAsync("Test", new Message<int, UserUpdated> { Key = i, Value = user }, cancellationToken)
                        .ContinueWith(task =>
                            {
                                if (!task.IsFaulted)
                                {
                                    Console.WriteLine($"produced to: {task.Result.TopicPartitionOffset}");
                                }
                                else
                                {

                                   // Task.Exception is of type AggregateException. Use the InnerException property
                                   // to get the underlying ProduceException. In some cases (notably Schema Registry
                                   // connectivity issues), the InnerException of the ProduceException will contain
                                   // additional information pertaining to the root cause of the problem. Note: this
                                   // information is automatically included in the output of the ToString() method of
                                   // the ProduceException which is called implicitly in the below.
                                   Console.WriteLine($"error producing message: {task.Exception.InnerException}");
                                }
                            }, cancellationToken);

                    await Task.Delay(1000, cancellationToken);
                }
            }
      }
   }
}
