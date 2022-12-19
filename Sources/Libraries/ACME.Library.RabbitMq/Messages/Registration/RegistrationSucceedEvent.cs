using ACME.Library.RabbitMq.Attributes;
using ACME.Library.Domain.Registration;

namespace ACME.Library.RabbitMq.Messages.Registration
{
    [BusTypeName(nameof(RegistrationSucceedEvent))]
    public class RegistrationSucceedEvent : RegistrationData
    {
    }
}