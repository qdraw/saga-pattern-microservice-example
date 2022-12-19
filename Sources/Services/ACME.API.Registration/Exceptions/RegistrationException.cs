using Microsoft.Extensions.Logging;
using System;

namespace ACME.API.Registration.Exceptions
{
    public abstract class RegistrationException : Exception
    {
        public virtual string ErrorCode => $"ACME.REGISTRATION.{ErrorCodeId:000}";
        protected abstract int ErrorCodeId { get; }
        public abstract int StatusCode { get; }
        public abstract LogLevel LogLevel { get; }

        public RegistrationException()
        {
        }

        protected RegistrationException(string message)
            : base(message)
        {
        }

        protected RegistrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
