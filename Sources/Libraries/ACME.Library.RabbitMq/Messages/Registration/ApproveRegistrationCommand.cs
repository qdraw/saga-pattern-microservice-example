using ACME.Library.RabbitMq.Attributes;
using System;

namespace ACME.Library.RabbitMq.Messages.Registration
{
    [BusTypeName(nameof(ApproveRegistrationCommand))]
    public class ApproveRegistrationCommand
    {
        public Guid CorrelationId { get; set; }
    }    
}

