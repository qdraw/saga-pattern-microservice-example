using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ACME.Library.Common.Bus;
using ACME.Library.Saga.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ACME.Library.Saga
{
    public class SagaHandler : IHostedService
    {
        private readonly ILogger<SagaHandler> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageBus _bus;
        private readonly IReadOnlyCollection<SagaConfiguration> _sagaConfigurations;

        public SagaHandler(ILogger<SagaHandler> logger, IServiceProvider serviceProvider, IMessageBus bus, IEnumerable<SagaConfiguration> sagaConfigurations)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _bus = bus;
            _sagaConfigurations = sagaConfigurations.ToList();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach(var configuration in _sagaConfigurations)
            {
                var sagaMessageSubscriptionTypes = GetMessageHandlerTypeArguments(configuration.SagaType);

                foreach (var messageType in sagaMessageSubscriptionTypes)
                {
                    async Task<bool> Subscribe()
                    {
                        await SubscribeOnMessage(messageType);
                        return true;
                    }
                    
                    await Common.Helpers.RetryHelper.DoAsync(Subscribe,TimeSpan.FromSeconds(15));
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private IEnumerable<Type> GetCorrespondingSagasByMessageType(Type messageType)
        {
            var handlerType = typeof(IMessageHandler<>).MakeGenericType(messageType);
            var correspondingSagas = _sagaConfigurations.Where(c => handlerType.IsAssignableFrom(c.SagaType)).Select(c => c.SagaType);
            return correspondingSagas;
        }

        private IEnumerable<Type> GetMessageHandlerTypeArguments(Type sagaType)
        {
            var interfaces = sagaType.GetInterfaces();
            var messageHandlers = interfaces.Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IMessageHandler<>));
            var messageTypes = messageHandlers.Select(t => t.GenericTypeArguments.First()).ToList();

            return messageTypes;
        }

        private async Task OnMessageReceived<TMessage>(TMessage message)
            where TMessage: class
        {
            using var scope = _serviceProvider.CreateScope();
            var sagaTypes = GetCorrespondingSagasByMessageType(message.GetType());

            foreach (var sagaType in sagaTypes)
            {
                await InvokeSagaMessageHandlerAsync(scope, sagaType, message);
            }
        }

        private async Task InvokeSagaMessageHandlerAsync<TMessage>(IServiceScope scope, Type sagaType, TMessage message)
            where TMessage : class
        {
            var sagaInstance = (ISaga)scope.ServiceProvider.GetService(sagaType);
            var method = sagaType.GetMethod(nameof(IMessageHandler<object>.HandleAsync), new[] { message.GetType() });

            try
            {
                sagaInstance!.Initialize(message);
                var awaitable = (Task)method?.Invoke(sagaInstance, new[] { message });
                await awaitable!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while handing message of type '{message.GetType()}'");
                throw;
            }
        }

        private async Task SubscribeOnMessage(Type messageType)
        {
            var typeArgs = new Type[] { messageType, typeof(Task) };
            var delegMethodType = typeof(Func<,>).MakeGenericType(typeArgs);
            var methodInfo = typeof(SagaHandler).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(m => m.Name == nameof(OnMessageReceived))
                .MakeGenericMethod(messageType);

            var delegMethod = Delegate.CreateDelegate(delegMethodType, this, methodInfo);

            var subscriptionTask = (Task)typeof(IMessageBus)
                .GetMethods()
                .First(x => x.Name == nameof(IMessageBus.ConsumeAsync) && x.GetParameters().Count() == 1)
                .MakeGenericMethod(messageType)
                .Invoke(_bus, new object[]
                {
                    delegMethod
                });

            await subscriptionTask!;
        }
    }
}