using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using PMDEvers.Servicebus;

namespace Servicebus.Kafka
{
   public class EventProducerEventHandler<TEvent> : ICancellableAsyncEventHandler<TEvent>
      where TEvent : IEvent
   {
      private readonly SchemaRegistryConfig _schemaRegistryConfig;
      private readonly ProducerConfig _producerConfig;

      public EventProducerEventHandler(SchemaRegistryConfig schemaRegistryConfig, ProducerConfig producerConfig)
      {
         _schemaRegistryConfig = schemaRegistryConfig;
         _producerConfig = producerConfig;
      }

      public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken)
      {
         if (typeof(TEvent).GetCustomAttribute<Produce>() == null)
         {
            return;
         }

         using var schemaRegistry = new CachedSchemaRegistryClient(_schemaRegistryConfig);
         using var producer =
             new ProducerBuilder<string, TEvent>(_producerConfig)
                 .SetKeySerializer(new AvroSerializer<string>(schemaRegistry))
                 .SetValueSerializer(new AvroSerializer<TEvent>(schemaRegistry))
                 .Build();
         var topic = typeof(TEvent).GetCustomAttribute<KafkaTopic>().TopicName;
         await producer
             .ProduceAsync(topic, new Message<string, TEvent> { Key = "1", Value = @event }, cancellationToken)
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
      }

      
   }
}
