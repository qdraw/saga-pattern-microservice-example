using EasyNetQ;
using EasyNetQ.Consumer;
using Microsoft.Extensions.DependencyInjection;
using ACME.Library.Common.Bus;
using ACME.Library.RabbitMq.Configuration;
using ACME.Library.RabbitMq.Serializer;
using ACME.Library.RabbitMq.Services;
using ACME.Library.RabbitMq.Strategies;

namespace ACME.Library.RabbitMq.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRabbitMq(this IServiceCollection services, string connectionString, DeadLetterConfiguration deadLetterConfiguration)
        {
            services.AddSingleton<IMessageBus, RabbitMqService>();
            services.AddSingleton(RabbitHutch.CreateBus(connectionString, x =>
            {
                x.Register(deadLetterConfiguration);
                x.Register<IConsumerErrorStrategy, NackConsumerErrorStrategy>();
                x.Register<ITypeNameSerializer>(new CustomEasyNetQTypeNameSerializer());
            }));
        }
    }
}