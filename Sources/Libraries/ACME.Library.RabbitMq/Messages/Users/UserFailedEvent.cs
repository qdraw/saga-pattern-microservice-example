using ACME.Library.Domain.Interfaces.Saga;
using ACME.Library.RabbitMq.Attributes;
using System;
using ACME.Library.Domain.Registration;

namespace ACME.Library.RabbitMq.Messages.Users
{
    [BusTypeName(nameof(UserFailedEvent))]
    public class UserFailedEvent : RegistrationData, ISagaData
    {
        //Guid veplaatsen naar RegistrationContact model?
        public Guid UserId { get; set; }
        public string CompanyCode { get; set; }
        public string Reason { get; set; }
    }    
}