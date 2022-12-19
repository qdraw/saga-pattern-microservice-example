using ACME.Library.Domain.Registration;
using ACME.Library.RabbitMq.Attributes;

namespace ACME.Library.RabbitMq.Messages.Registration
{
    [BusTypeName(nameof(RegistrationSubmittedEvent))]
    public class RegistrationSubmittedEvent : RegistrationData
    {
        // nothing here
    }
}