using System;

namespace ACME.Library.Saga.Exceptions
{
    public class EntityAlreadyExistsException : SagaException
    {
        public EntityAlreadyExistsException(Guid correlationId)
            : base($"Entity with CorrelationId '{correlationId}' already exists")
        {
        }
    }
}