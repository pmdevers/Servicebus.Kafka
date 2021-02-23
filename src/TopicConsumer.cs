using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avro.Generic;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using PMDEvers.Servicebus;

namespace Servicebus.Kafka
{
   public class TopicConsumer
   {
      private readonly IServiceProvider _serviceProvider;
      private readonly IServiceCollection _services;
      private ConsumerConfig _consumerConfig;
      private SchemaRegistryConfig _schemaRegistryConfig;

      public TopicConsumer(IServiceProvider serviceProvider, IServiceCollection services)
      {
         _serviceProvider = serviceProvider;
         _services = services;
         _consumerConfig = serviceProvider.GetRequiredService<ConsumerConfig>();
         _schemaRegistryConfig = serviceProvider.GetRequiredService<SchemaRegistryConfig>();
      }


      public async Task HandleTopicAsync(string topic, CancellationToken token)
      {
         var types = GetTypes(topic);

         using var schemaRegistry = new CachedSchemaRegistryClient(_schemaRegistryConfig);
         using var consumer = new ConsumerBuilder<int, GenericRecord>(_consumerConfig)
            .SetKeyDeserializer(new AvroDeserializer<int>(schemaRegistry).AsSyncOverAsync())
            .SetValueDeserializer(new AvroDeserializer<GenericRecord>(schemaRegistry).AsSyncOverAsync())
            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
            .Build();
         consumer.Subscribe(topic);

         try
         {
            while (true)
            {
               try
               {
                  var cr = consumer.Consume(token);

                  if (cr.Message != null)
                  {
                     var type = types.FirstOrDefault(x => x.FullName == cr.Message.Value.Schema.Fullname);
                     var serializer = new AvroSerializer<GenericRecord>(schemaRegistry);
                     var bytes = await serializer.SerializeAsync(cr.Message.Value, new SerializationContext());
                     var handler = (IAvroEventHandler) _serviceProvider.GetService(typeof(AvroEventHandler<>).MakeGenericType(type));
                     await handler.Handle(bytes, schemaRegistry, token);
                  }
               }
               catch (ConsumeException ex)
               {

               }
            }
         }  catch (OperationCanceledException)
         {
            consumer.Close();
         }
      }


      private IEnumerable<Type> GetTypes(string topic)
      {
         return _services.Where(s =>
               s.ServiceType.IsGenericType &&
               s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
            .Select(s => s.ServiceType.GetGenericArguments()[0])
            .Where(x => x.GetCustomAttribute<KafkaTopic>().TopicName == topic);
      }
   }
}
