using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ACME.API.Registration.Exceptions
{
    public class ValidationFailedException : RegistrationException
    {
        protected override int ErrorCodeId => 1;

        public override int StatusCode => StatusCodes.Status400BadRequest;

        public override LogLevel LogLevel => LogLevel.Warning;

        public List<ValidationFailure> Errors { get; set; }

        public ValidationFailedException(List<ValidationFailure> errors)
            : base("There are validation errors")
        {
            Errors = errors;
        }
    }
}