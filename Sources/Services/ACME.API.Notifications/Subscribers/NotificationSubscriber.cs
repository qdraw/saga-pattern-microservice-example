using ACME.API.Notifications.Services.Interfaces;
using ACME.Library.Common.Bus;

namespace ACME.API.Notifications.Subscribers
{
    public class NotificationSubscriber : IHostedService
    {
        private readonly IServiceProvider _provider;

        public NotificationSubscriber(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Listens for events that does not require a direct result
        /// </summary>
        /// <param name="cancellationToken">cancel token</param>
        /// <exception cref="Exception">rabbit mq service is missing</exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();
            var bus = scope.ServiceProvider.GetService<IMessageBus>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<NotificationSubscriber>>();
            if (bus == null)
            {
                throw new Exception("rabbit mq service is missing");
            }

            try
            {
                // Listen here for events
                // await bus.ConsumeAsync<SignalREvent>(r =>
                //     ExecuteWithServiceAsync<IMessageService>(s => s.SendToAll(r.Notification)));
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException e)
            {
                logger.LogError(e,"rabbit mq failed listening");
            }
            
        }
        private async Task ExecuteWithServiceAsync<T>(Func<T, Task> delegateAction) where T : notnull
        {
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<T>();
            await delegateAction(service);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}