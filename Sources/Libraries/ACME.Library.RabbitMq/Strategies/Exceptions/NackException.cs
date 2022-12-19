using System;

namespace ACME.Library.RabbitMq.Strategies.Exceptions
{
    public class NackException : Exception
    {
        public HandlingStrategy HandlingStrategy { get; set; }

        public NackException(HandlingStrategy handlingStrategy, string message)
            : base(message)
        {
            HandlingStrategy = handlingStrategy;
        }

        public NackException(HandlingStrategy handlingStrategy, string message, Exception innerException)
            : base(message, innerException)
        {
            HandlingStrategy = handlingStrategy;
        }

        public NackException(HandlingStrategy handlingStrategy, Exception innerException)
            : this(handlingStrategy, innerException.Message, innerException)
        {
        }
    }
}