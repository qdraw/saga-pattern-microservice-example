using EasyNetQ;
using Microsoft.Extensions.Logging;
using ACME.Library.Common.Bus;
using System;
using System.Threading.Tasks;

namespace ACME.Library.RabbitMq.Services
{
    public class RabbitMqService : IMessageBus
    {
        private const ushort DEFAULT_PREFETCH_COUNT = 1;

        private readonly ILogger<RabbitMqService> _logger;
        private readonly IBus _bus;
        private readonly string subscriptionId;

        public RabbitMqService(ILogger<RabbitMqService> logger, IBus bus)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            subscriptionId = AppDomain.CurrentDomain.FriendlyName;
        }

        public async Task PublishAsync<T>(T message)
        {
            _logger.LogInformation($"Publishing '{message.GetType().Name}'");
            await _bus.PubSub.PublishAsync(message, message.GetType());
        }

        public async Task PublishAsync<T>(T message, string topic)
        {
            _logger.LogInformation($"Publishing '{message.GetType().Name}' on topic '{topic}'");
            await _bus.PubSub.PublishAsync(message, message.GetType(), topic);
        }

        public async Task ConsumeAsync<TMessage>(Func<TMessage, Task> delegateAction, ushort prefetchCount)
        {
            await _bus.PubSub.SubscribeAsync<TMessage>(subscriptionId,
                (msg, _) =>
                {
                    AssertMessageNotNull(msg);

                    _logger.LogInformation($"Consuming '{msg.GetType().Name}'");

                    return delegateAction(msg);
                }, cfg => cfg.WithPrefetchCount(prefetchCount));
            _logger.LogInformation($"Subscribed on '{typeof(TMessage)}'");
        }

        public Task ConsumeAsync<TMessage>(Func<TMessage, Task> delegateAction)
        {
            return ConsumeAsync(delegateAction, DEFAULT_PREFETCH_COUNT);
        }

        public async Task ConsumeAsync<TMessage>(Func<TMessage, Task> delegateAction, string topic, ushort prefetchCount = DEFAULT_PREFETCH_COUNT)
        {
            await _bus.PubSub.SubscribeAsync<TMessage>($"{topic}_{subscriptionId}",
                (msg, _) =>
                {
                    AssertMessageNotNull(msg);

                    _logger.LogInformation($"Consuming '{msg.GetType().Name}' via topic {topic}");

                    return delegateAction(msg);
                }, cfg => cfg.WithTopic(topic).WithPrefetchCount(prefetchCount));
            _logger.LogInformation($"Subscribed on '{typeof(TMessage)}' with topic '{topic}'");
        }

        public Task ConsumeAsync<TMessage>(Func<TMessage, Task> delegateAction, string topic)
        {
            return ConsumeAsync(delegateAction, topic, DEFAULT_PREFETCH_COUNT);
        }

        public Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request)
        {
            _logger.LogInformation($"Requests '{typeof(TRequest)}'");
            return _bus.Rpc.RequestAsync<TRequest, TResponse>(request);
        }

        public async Task RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> delegateAction)
        {
            await _bus.Rpc.RespondAsync<TRequest, TResponse>(
                request =>
                {
                    if (request == null)
                        throw new Exception("Empty msg, could not process.");

                    _logger.LogInformation($"Responding to '{request.GetType().Name}'");

                    return delegateAction(request);
                }
            );

            _logger.LogInformation($"Responds on '{typeof(TRequest)}'");
        }

        private void AssertMessageNotNull<TRequest>(TRequest msg)
        {
            if (msg == null)
            {
                throw new Exception("Empty msg, could not process.");
            }
        }
    }
}