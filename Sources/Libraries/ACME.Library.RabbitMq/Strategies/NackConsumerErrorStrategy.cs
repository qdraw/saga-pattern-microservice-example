using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Logging;
using ACME.Library.RabbitMq.Configuration;
using ACME.Library.RabbitMq.Strategies.Exceptions;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACME.Library.RabbitMq.Strategies
{
    public class NackConsumerErrorStrategy : DefaultConsumerErrorStrategy
    {
        private readonly ILog logger = LogProvider.For<NackConsumerErrorStrategy>();
        private readonly IPersistentConnection connection;
        private readonly DeadLetterConfiguration deadLetterConfiguration;

        public NackConsumerErrorStrategy(IPersistentConnection connection, ISerializer serializer, IConventions conventions, ITypeNameSerializer typeNameSerializer, IErrorMessageSerializer errorMessageSerializer, ConnectionConfiguration configuration, DeadLetterConfiguration deadLetterConfiguration)
            : base(connection, serializer, conventions, typeNameSerializer, errorMessageSerializer, configuration)
        {
            this.connection = connection;
            this.deadLetterConfiguration = deadLetterConfiguration;
        }

        public override AckStrategy HandleConsumerCancelled(ConsumerExecutionContext context)
        {
            return HandleException(context, new NackException(HandlingStrategy.NackDelayedRequeue, new OperationCanceledException()));
        }

        public override AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
            return HandleException(context, exception);
        }

        private AckStrategy HandleException(ConsumerExecutionContext context, Exception exception)
        {
            logger.Log(LogLevel.Error, () => $"Unhandled exception thrown while processing message of type '{context.Properties.Type}': {exception.Message}", exception);

            if (!(exception is NackException nackException))
                return base.HandleConsumerError(context, exception);

            if (nackException.HandlingStrategy == HandlingStrategy.NackImmediateRequeue)
                return AckStrategies.NackWithRequeue;

            if (TryGetRetryCount(context.Properties.Headers, out var retryCount) && retryCount > deadLetterConfiguration.MaximumRetries)
            {
                logger.Log(LogLevel.Error, () => $"Unable to process message with correlation-id {context.Properties.CorrelationId}. Moving message to the error queue");
                return base.HandleConsumerError(context, exception);
            }
            MessageDelayedRequeue(context);

            return AckStrategies.NackWithoutRequeue;
        }

        private bool TryGetRetryCount(IDictionary<string, object> headers, out long retryCount)
        {
            retryCount = 0;

            if (!headers.TryGetValue("x-death", out var retryHeader) ||
                !(retryHeader is IList<object> retryHeaderList) || !retryHeaderList.Any() ||
                !(retryHeaderList.First() is IDictionary<string, object> kvpHeader) ||
                !kvpHeader.TryGetValue("count", out var retryCountObject) ||
                !(retryCountObject is long value)) 
                return false;
            
            retryCount = value;
            return true;
        }

        private void MessageDelayedRequeue(ConsumerExecutionContext context)
        {
            var model = connection.CreateModel();

            var dlName = $"{context.ReceivedInfo.Exchange}_dl_{(int)deadLetterConfiguration.RetryDelay.TotalMilliseconds}";
            DeclareDeadLetterExchange(model, dlName);
            DeclareDeadLetterQueue(model, dlName, context.ReceivedInfo.Exchange, deadLetterConfiguration.RetryDelay);

            // Bind dead-letter exchange to dead-letter queue
            model.QueueBind(dlName, dlName, "#");

            PublishMessage(model, dlName, context.ReceivedInfo.RoutingKey, context.Properties, context.Body);
        }

        private static void PublishMessage(IModel model, string exchangeName, string routingKey, MessageProperties properties, byte[] body)
        {
            var data = new ReadOnlyMemory<byte>(body);
            var basicProperties = model.CreateBasicProperties();
            properties.CopyTo(basicProperties);
            model.BasicPublish(exchangeName, routingKey, true, basicProperties, data);
        }

        private static void DeclareDeadLetterQueue(IModel model, string queueName, string receivedMessageExchange, TimeSpan retryDelay)
        {
            var queueArguments = new Dictionary<string, object>
            {
                {"x-dead-letter-exchange", receivedMessageExchange},
                {"x-message-ttl", (int)retryDelay.TotalMilliseconds}
            };
            model.QueueDeclare(queueName, true, false, false, queueArguments);
        }

        private static void DeclareDeadLetterExchange(IModel model, string exchangeName)
        {
            model.ExchangeDeclare(
                exchangeName,
                ExchangeType.Topic,
                true
            );
        }
    }
}