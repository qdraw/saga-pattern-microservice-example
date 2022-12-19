namespace ACME.Library.Common.Outbox
{
    public interface IOutboxProcessor
    {
        void ProcessOutbox();
    }
}