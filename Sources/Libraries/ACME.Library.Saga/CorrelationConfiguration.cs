using System;

namespace ACME.Library.Saga
{
    internal abstract class CorrelationConfiguration
    {
    }

    internal class CorrelationConfiguration<TMessage> : CorrelationConfiguration
    {
        public Func<TMessage, Guid> MessageValueExtractorFunction { get; }

        public CorrelationConfiguration(Func<TMessage, Guid> messageValueExtractorFunction)
        {
            MessageValueExtractorFunction = messageValueExtractorFunction;
        }

        public Guid GetCorrelationId(TMessage data)
        {
            return MessageValueExtractorFunction(data);
        }
    }
}