using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PMDEvers.Servicebus;
using Xunit;



namespace Servicebus.Kafka.Test
{
    public class UnitTest1
    {
        [Fact]
        public void DiscoverHandlersWithAttribute()
        {
            var services = new ServiceCollection();

            services.AddServiceBus()
                .AddEventHandler<TestEvent, TestEventHandler>();

            var provider = services.BuildServiceProvider();

            provider.GetServices<IEventHandler<IEvent>>();



            var hand = services
                .Where(s =>
                    s.ServiceType.IsGenericType &&
                    s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>));
                





            var messageTypesWithNotificationHandlers = services
                .Where(s => s.ServiceType.IsGenericType &&
                            s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(s => s.ServiceType.GetGenericArguments()[0])
                .Where(s => s.IsGenericType &&
                            s.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(s => s.GetGenericArguments()[0])
                .Where(s => typeof(IEvent).IsAssignableFrom(s))
                .Distinct();

            var handlers = messageTypesWithNotificationHandlers
                .SelectMany(Attribute.GetCustomAttributes)
                .OfType<KafkaTopic>()
                .Select(t => t.TopicName)
                .Distinct()
                .ToList();

        }

        [Fact]
        public void CreatType()
        {
            var services = new ServiceCollection();

            services.AddServiceBus()
                .AddEventHandler<TestEvent, TestEventHandler>();


            var service = services
                .Where(s => 
                    s.ServiceType.IsGenericType &&
                    s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(s=> s.ServiceType.GetGenericArguments()[0])
                .Where(x=> x.GetCustomAttribute<KafkaTopic>().TopicName == "")
                .Distinct();
           

            var getTopics = services
               .Where(s => 
                  s.ServiceType.IsGenericType &&
                  s.ServiceType.GetGenericTypeDefinition() == typeof(IEventHandler<>))
               .Select(s=> s.ServiceType.GetGenericArguments()[0])
               .Select(x=> x.GetCustomAttribute<KafkaTopic>())
               .Select(x=> x.TopicName)
               .Distinct();




        }
    }
}
