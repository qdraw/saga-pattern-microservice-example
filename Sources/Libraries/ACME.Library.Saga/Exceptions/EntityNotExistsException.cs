using System;

namespace ACME.Library.Saga.Exceptions
{
    public class EntityNotExistsException : SagaException
    {
        public EntityNotExistsException(Guid correlationId)
            : base($"Entity with CorrelationId '{correlationId}' could not be found")
        {
        }
    }
}