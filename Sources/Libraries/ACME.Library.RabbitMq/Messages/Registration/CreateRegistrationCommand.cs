using ACME.Library.Domain.Registration;
using ACME.Library.RabbitMq.Attributes;

namespace ACME.Library.RabbitMq.Messages.Registration;

[BusTypeName(nameof(CreateRegistrationCommand))]
public class CreateRegistrationCommand : RegistrationData
{ 
    // nothing here due inheritance
}