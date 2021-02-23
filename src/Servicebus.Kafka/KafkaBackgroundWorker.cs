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
               Task.Run(() => new TopicConsumer(_provider, _services).HandleTopicAsync(consumer, cancellationToken), cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private IEnumerable<string> GetTopicConsumers()
        {
            var topics = _services
                .Where(s =>
                    s.ServiceType.IsGenericType &&
                    s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(s => s.ServiceType.GetGenericArguments()[0])
                .Select(x => x.GetCustomAttribute<KafkaTopic>().TopicName)
                .Distinct();

            return topics;
        }
    }
}
