using ACME.Library.Domain.Registration;
using ACME.Library.RabbitMq.Attributes;

namespace ACME.Library.RabbitMq.Messages.Registration
{
    [BusTypeName(nameof(RegistrationApprovedEvent))]
    public class RegistrationApprovedEvent : RegistrationData
    {
        // nothing here
    }
}