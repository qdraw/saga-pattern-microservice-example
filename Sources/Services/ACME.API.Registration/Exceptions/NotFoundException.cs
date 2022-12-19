using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ACME.API.Registration.Enums;
using System;

namespace ACME.API.Registration.Exceptions
{
    public class NotFoundException<T> : RegistrationException
    {
        protected override int ErrorCodeId => 100 + ResourceId;

        public override int StatusCode => StatusCodes.Status404NotFound;

        public override LogLevel LogLevel => LogLevel.Error;

        public int ResourceId => (int)Enum.Parse(typeof(RegistrationExceptionType), typeof(T).Name);

        public NotFoundException(string message)
            : base($"{typeof(T).Name} not found, request: {message}")
        {
        }
    }
}