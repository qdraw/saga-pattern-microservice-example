using ACME.Library.Domain.Interfaces.Saga;
using ACME.Library.RabbitMq.Attributes;
using System;
using ACME.Library.Domain.Registration;

namespace ACME.Library.RabbitMq.Messages.Users
{
    [BusTypeName(nameof(UserCreatedEvent))]
    public class UserCreatedEvent : RegistrationData, ISagaData
    {
        public Guid UserId { get; set; }
        public string CompanyCode { get; set; }
    }    
}

