using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PMDEvers.Servicebus;

namespace Servicebus.Kafka
{
    public class KafkaBackgroundWorker : IHostedService
    {
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _provider;

        public KafkaBackgroundWorker(IServiceCollection services, IServiceProvider provider)
        {
            _services = services;
            _provider = provider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var consumers = GetTopicConsumers();

            foreach(var consumer in consumers)
            {
                new Thread(() => consumer.StartConsuming(_provider, cancellationToken))
                    .Start();
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<ITopicEventConsumer> GetTopicConsumers()
        {
            var service = _services
                .Where(s =>
                    s.ServiceType.IsGenericType &&
                    s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(s => s.ServiceType.GetGenericArguments()[0])
                .Where(x => x.GetCustomAttribute<KafkaTopic>() != null)
                .Distinct();

            var consumers = service.Select(eventType =>
                (ITopicEventConsumer)Activator.CreateInstance(
                    typeof(TopicEventConsumer<>).MakeGenericType(eventType)));

            return consumers;
        }
    }
}
