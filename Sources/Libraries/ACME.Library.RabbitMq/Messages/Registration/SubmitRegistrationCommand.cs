using ACME.Library.RabbitMq.Attributes;
using System;
using System.Collections.Generic;
using ACME.Library.Domain.Registration;

namespace ACME.Library.RabbitMq.Messages.Registration
{
    [BusTypeName(nameof(SubmitRegistrationCommand))]
    public class SubmitRegistrationCommand : RegistrationData
    {
    }
}