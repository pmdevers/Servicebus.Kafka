using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PMDEvers.Servicebus;
using Xunit;

namespace Servicebus.Kafka.Test
{
    public class RegistrationTests
    {
        [Fact]
        public void ThrowsException_If_ServiceBus_Is_Not_Registered()
        {
            var services = new ServiceCollection();

            Assert.Throws<NotImplementedException>(() => 
                services.AddServiceBusKafka(
                    new ConsumerConfig(),
                    new SchemaRegistryConfig())
                );
        }

        [Fact]
        public void KafkaBackgroundWorker_Is_Registered()
        {
            var services = new ServiceCollection();

            services.AddServiceBus();

            services.AddServiceBusKafka(new ConsumerConfig(),
                new SchemaRegistryConfig());

            var provider = services.BuildServiceProvider();

            var worker = provider.GetService<IHostedService>();

            Assert.NotNull(worker);
            Assert.IsType<KafkaBackgroundWorker>(worker);
        }

        [Fact]
        public async Task KafkaBackgroundWorker()
        {
            var services = new ServiceCollection();

            services.AddServiceBus()
                .AddEventHandler<TestEvent, TestEventHandler>();

            services.AddServiceBusKafka(new ConsumerConfig(),
                new SchemaRegistryConfig());

            var provider = services.BuildServiceProvider();

            var worker = provider.GetRequiredService<IHostedService>();

            await worker.StartAsync(CancellationToken.None);
        }
    }
}
