using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ACME.API.Registration.Models;
using ACME.API.Registration.Services.Interfaces;
using ACME.Library.Common.Helpers;
using ACME.Library.Common.Models.Api;
using ACME.Library.Domain.Enums.Registration;
using ACME.Library.RabbitMq.Messages.Registration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ACME.Library.Domain.Registration;
using ACME.Library.RabbitMq.Messages.Registration;

namespace ACME.API.Registration.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly IQueueRegistrationService _queueRegistrationService;

        public RegistrationController(IRegistrationService registrationService,
                                      IQueueRegistrationService queueRegistrationService)
        {
            _registrationService = registrationService;
            _queueRegistrationService = queueRegistrationService;
        }
        
        /// <summary>
        /// Create registration
        /// </summary>
        /// <remarks>
        /// Sample request (minimal information):
        /// {
        /// "name": "test",
        /// "email": "test@test.com"
        /// }
        ///
        ///  Use correlationId in next requests to get registration status or approve registration
        /// </remarks>
        [HttpPut("create")]
        public async Task<ActionResult<ApiResponse<RegistrationSubmittedIdsResult>>> CreateRegistration(RegistrationData registration)
        {
            if (registration.CorrelationId == Guid.Empty || registration.CorrelationId == new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"))
            {
                registration.CorrelationId = Guid.NewGuid();
            }
            
            await _queueRegistrationService.CreateRegistration(registration);

            Response.StatusCode = (int)System.Net.HttpStatusCode.Accepted;
            return ApiResponse<RegistrationSubmittedIdsResult>.Success(new RegistrationSubmittedIdsResult
            {
                CorrelationId = registration.CorrelationId,
            });
        }


        /// <summary>
        /// Get specific registration
        /// </summary>
        /// <remarks>
        ///     the correlationId of the registration
        /// </remarks>
        /// <param name="correlationId">The correlationId of the registration</param>
        [HttpGet("{correlationId}")]
        public async Task<ActionResult<ApiResponse<RegistrationData>>> GetRegistration(Guid correlationId)
        {
            var result = await _registrationService.GetRegistration(correlationId);
            return ApiResponse<RegistrationData>.Success(result);
        }

        /// <summary>
        /// Approve registration
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="correlationId">The correlationId of the registration</param>
        [HttpPost("approve/{correlationId}")]
        public async Task<ActionResult<Guid>> ApproveRegistration(Guid correlationId)
        {
            await _queueRegistrationService.ApproveRegistration(new ApproveRegistrationCommand
            {
                CorrelationId = correlationId,
            });
            
            return Accepted(new Uri("registration", UriKind.Relative),correlationId);
        }

    }
}
