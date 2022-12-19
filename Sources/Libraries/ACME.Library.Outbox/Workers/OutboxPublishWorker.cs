using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using ACME.Library.Common.Outbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ACME.Library.Outbox.Workers
{
    public class OutboxPublishWorker : BackgroundService, IOutboxProcessor
    {
        private readonly AsyncAutoResetEvent _waitHandle = new AsyncAutoResetEvent(false);
        private readonly IServiceProvider _serviceProvider;

        public OutboxPublishWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _waitHandle.WaitAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (TaskCanceledException e)
                {
                }
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var outboxPublisher = (IOutboxPublisher)scope.ServiceProvider.GetService(typeof(IOutboxPublisher));
                    await outboxPublisher.PublishMessagesAsync();
                }
            }
        }

        public void ProcessOutbox()
        {
            _waitHandle.Set();
        }
    }
}