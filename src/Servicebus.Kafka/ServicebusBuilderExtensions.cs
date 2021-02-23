using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using PMDEvers.Servicebus;

namespace Servicebus.Kafka
{
    public static class ServicebusBuilderExtensions
    {
        public static IServiceCollection AddServiceBusKafka(
            this IServiceCollection services, 
            ConsumerConfig consumerConfig,
            SchemaRegistryConfig schemaRegistryConfig)
        {
            if (services.All(x => x.ServiceType != typeof(IServiceBus)))
            {
                throw new NotImplementedException("Please use services.AddServicebus() first.");
            }

            services.AddSingleton(new ProducerConfig()
            {
               BootstrapServers = consumerConfig.BootstrapServers
            });
            services.AddSingleton(consumerConfig);
            services.AddSingleton(schemaRegistryConfig);
            services.AddHostedService(s => new KafkaBackgroundWorker(services, s));
            services.AddTransient(typeof(AvroEventHandler<>));
            
            return services;
        }

        public static ServiceBusBuilder ProduceEvent<TEvent>(this ServiceBusBuilder builder)
            where TEvent : IEvent, ISpecificRecord
        {
           return builder.AddEventHandler<TEvent, EventProducerEventHandler<TEvent>>();
        }
    }
}
