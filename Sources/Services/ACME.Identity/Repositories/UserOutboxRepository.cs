using Microsoft.EntityFrameworkCore;
using ACME.Identity.Data;
using ACME.Library.Common.Outbox;
using ACME.Library.Outbox.EntityFramework.Entities;

namespace ACME.Identity.Repositories
{
    public class UserOutboxRepository : IOutboxRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserOutboxRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddMessageAsync(IOutboxMessage message)
        {
            await _dbContext.OutboxMessages.AddAsync((OutboxMessage)message);
        }

        public async Task<IEnumerable<IOutboxMessage>> GetUnpublishedMessagesAsync()
        {
            return await _dbContext.OutboxMessages.Where(m => 
                m.State != OutboxMessageState.Shipped).OrderBy(m => m.Id).ToListAsync();
        }

        public void UpdateMessageState(IOutboxMessage message, OutboxMessageState newState)
        {
            var outboxMessage = (OutboxMessage)message;
            outboxMessage.State = newState;
            _dbContext.OutboxMessages.Update(outboxMessage);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}