using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using PMDEvers.Servicebus;

namespace Servicebus.Kafka
{
   public class AvroEventHandler<TEvent> : IAvroEventHandler
      where TEvent : ISpecificRecord, IEvent
   {
      private readonly IServiceProvider _services;

      public AvroEventHandler(IServiceProvider services)
      {
         _services = services;
      }


      public async Task Handle(byte[] message, ISchemaRegistryClient schemaRegistry, CancellationToken token)
      {
         var deserializer = new AvroDeserializer<TEvent>(schemaRegistry); 
         var mySpecificRecord = await deserializer.DeserializeAsync(message, false, new SerializationContext());

         using (var scope = _services.CreateScope())
         {
            var serviceBus = scope.ServiceProvider.GetRequiredService<IServiceBus>();

            await serviceBus.PublishAsync(mySpecificRecord, token);
         }
      }
   }

   public interface IAvroEventHandler
   {
      Task Handle(byte[] message, ISchemaRegistryClient schemaRegistry, CancellationToken token);
   }
}
