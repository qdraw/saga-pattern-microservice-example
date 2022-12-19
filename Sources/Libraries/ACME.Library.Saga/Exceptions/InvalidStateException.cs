using System.Collections.Generic;

namespace ACME.Library.Saga.Exceptions
{
    public class InvalidStateException : SagaException
    {
        public InvalidStateException(IEnumerable<string> expectedStates, string currentState)
            : base($"Entity is in an invalid state: {currentState}. Expected: {string.Join("|", expectedStates)}")
        {
        }
    }
}