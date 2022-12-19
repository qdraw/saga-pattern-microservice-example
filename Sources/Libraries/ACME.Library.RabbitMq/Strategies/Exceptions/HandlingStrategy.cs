namespace ACME.Library.RabbitMq.Strategies.Exceptions
{
    public enum HandlingStrategy
    {
        NackImmediateRequeue,
        NackDelayedRequeue
    }
}