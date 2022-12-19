using ACME.Library.RabbitMq.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ACME.Library.RabbitMq.Interfaces
{
    public interface IEventRepository
    {
        Task CreateAsync(EventEntity @event);
        Task<IEnumerable<EventEntity>> GetPendingEvents();
        Task UpdateAsync(EventEntity @event);
        Task RemoveAsync(EventEntity @event);
    }
}