using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using PMDEvers.Servicebus;

namespace Servicebus.Kafka
{
    public class TopicEventConsumer<TEvent> : ITopicEventConsumer
        where TEvent : IEvent
    {
        public void StartConsuming(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var topic = typeof(TEvent).GetCustomAttribute<KafkaTopic>().TopicName;
            var consumerConfig = serviceProvider.GetRequiredService<ConsumerConfig>();
            var schemaRegistryConfig = serviceProvider.GetRequiredService<SchemaRegistryConfig>();

            using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var consumer =
                new ConsumerBuilder<string, TEvent>(consumerConfig)
                    .SetKeyDeserializer(new AvroDeserializer<string>(schemaRegistry).AsSyncOverAsync())
                    .SetValueDeserializer(new AvroDeserializer<TEvent>(schemaRegistry).AsSyncOverAsync())
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build())
            {
                consumer.Subscribe(topic);

                try
                {
                    while (true)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cancellationToken);

                            using (var scope = serviceProvider.CreateScope())
                            {
                                var servicebus = scope.ServiceProvider.GetRequiredService<IServiceBus>();

                                servicebus.PublishAsync(consumeResult.Message.Value).GetAwaiter().GetResult();
                            }
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Consume error: {e.Error.Reason}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }

            

        }
    }

    public interface ITopicEventConsumer
    {
        void StartConsuming(IServiceProvider serviceProvider, CancellationToken cancellationToken);
    }
}
