using System.Threading.Tasks;

namespace ACME.Library.Common.Outbox
{
    public interface ITransactionalMessagePublisher
    {
        Task<IOutboxMessage> CreateMessageAsync(object data, string topic = default);
        void Publish();
    }
}