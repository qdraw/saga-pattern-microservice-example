using ACME.API.Registration.Exceptions;
using System;

namespace ACME.API.Registration.Extensions
{
    internal static class ExceptionExtensions
    {
        public static string GetErrorCode(this Exception exception)
        {
            return exception is RegistrationException registrationException ? registrationException.ErrorCode : new UnexpectedRegistrationException().ErrorCode;
        }
    }
}