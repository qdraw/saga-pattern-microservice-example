using ACME.Library.Domain.Interfaces.Saga;
using Microsoft.Extensions.DependencyInjection;
using ACME.Library.Saga.Abstractions;

namespace ACME.Library.Saga.Extensions
{
    public static class ContainerExtensions
    {
        public static void RegisterSaga<TSaga, TSagaData, TSagaRepository>(this IServiceCollection services)
            where TSagaRepository: class, ISagaStateRepository<TSagaData>
            where TSagaData : ISagaData
            where TSaga : class
        {
            services.AddScoped<ISagaStateRepository<TSagaData>, TSagaRepository>();
            services.AddScoped<TSaga>();
            services.AddSingleton<SagaConfiguration>(new SagaConfiguration(typeof(TSaga)));
        }
        
        public static void RegisterSagaHandler(this IServiceCollection container)
        {
            container.AddHostedService<SagaHandler>();
        }
    }
}