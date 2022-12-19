using System;

namespace ACME.API.Registration.Models
{
    public class RegistrationVerificationModel
    {
        public Guid CorrelationId { get; set; }
        public string Email { get; set; }
    }
}
