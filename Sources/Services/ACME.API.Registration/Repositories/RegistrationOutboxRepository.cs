using Microsoft.EntityFrameworkCore;
using ACME.API.Registration.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACME.Library.Common.Outbox;
using ACME.Library.Outbox.EntityFramework.Entities;

namespace ACME.API.Registration.Repositories
{
    public class RegistrationOutboxRepository : IOutboxRepository
    {
        private readonly RegistrationDbContext _dbContext;

        public RegistrationOutboxRepository(RegistrationDbContext dbContext)
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
