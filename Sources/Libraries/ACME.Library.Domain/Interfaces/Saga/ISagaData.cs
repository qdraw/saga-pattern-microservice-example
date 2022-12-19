using System;

namespace ACME.Library.Domain.Interfaces.Saga
{
    public interface ISagaData
    {
        /// <summary>
        /// Used to link the different services together
        /// </summary>
        Guid CorrelationId { get; set; }

        /// <summary>
        /// Depends on the service
        /// </summary>
        string State { get; set; }
    }
}