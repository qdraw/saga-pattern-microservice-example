using Microsoft.Extensions.DependencyInjection;
using ACME.Library.Common.Outbox;
using ACME.Library.Outbox.Workers;

namespace ACME.Library.Outbox.Extensions
{
    public static class ContainerExtensions
    {
        public static void RegisterOutboxPublishWorker(this IServiceCollection services)
        {
            services.AddSingleton<OutboxPublishWorker>();
            services.AddSingleton<IOutboxProcessor>(s => s.GetRequiredService<OutboxPublishWorker>());
            services.AddHostedService(s => s.GetRequiredService<OutboxPublishWorker>());
        }
    }
}
