namespace ACME.Library.Common.Outbox
{
    public interface IOutboxMessageFactory
    {
        IOutboxMessage Create(object data, string topic = default);
    }
}