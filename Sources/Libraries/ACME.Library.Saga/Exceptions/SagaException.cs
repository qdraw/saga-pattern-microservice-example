using System;

namespace ACME.Library.Saga.Exceptions
{
    public class SagaException : Exception
    {
        public SagaException(string message)
            : base(message)
        {
        }
    }
}