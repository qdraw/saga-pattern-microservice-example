using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ACME.API.Registration.Exceptions
{
    public class UnexpectedRegistrationException : RegistrationException
    {
        protected override int ErrorCodeId => 0;
        public override int StatusCode => StatusCodes.Status500InternalServerError;
        public override LogLevel LogLevel => LogLevel.Error;
        public UnexpectedRegistrationException() : base() { }
        public UnexpectedRegistrationException(string message)
           : base(message)
        {
        }
    }
}
