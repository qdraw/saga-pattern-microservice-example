using Microsoft.Extensions.DependencyInjection;
using ACME.Library.Common.Outbox;
using ACME.Library.Outbox;

namespace ACME.Library.Outbox.EntityFramework.Extensions
{
    public static class ContainerExtensions
    {
        public static void RegisterEntityFrameworkOutboxPublisher(this IServiceCollection services)
        {
            services.AddScoped<IOutboxPublisher, OutboxPublisher>();
            services.AddScoped<ITransactionalMessagePublisher, TransactionalMessagePublisher>();
            services.AddScoped<IOutboxMessageFactory, OutboxMessageFactory>();
        }
    }
}