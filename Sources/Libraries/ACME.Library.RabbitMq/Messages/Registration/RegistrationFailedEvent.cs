using ACME.Library.RabbitMq.Attributes;
using System;
using ACME.Library.Domain.Registration;

namespace ACME.Library.RabbitMq.Messages.Registration
{
    [BusTypeName(nameof(RegistrationFailedEvent))]
    public class RegistrationFailedEvent : RegistrationData
    {
        public string Reason { get; set; }
    }
}