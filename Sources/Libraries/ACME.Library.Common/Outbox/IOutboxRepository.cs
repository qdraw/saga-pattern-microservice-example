using System.Collections.Generic;
using System.Threading.Tasks;

namespace ACME.Library.Common.Outbox
{
    public interface IOutboxRepository
    {
        Task AddMessageAsync(IOutboxMessage message);
        void UpdateMessageState(IOutboxMessage message, OutboxMessageState newState);
        Task<IEnumerable<IOutboxMessage>> GetUnpublishedMessagesAsync();
        Task SaveChangesAsync();
    }
}