namespace ACME.Library.Saga.Exceptions
{
    public class InvalidConfigurationException : SagaException
    {
        public InvalidConfigurationException(string message)
            : base(message)
        {
        }
    }
}