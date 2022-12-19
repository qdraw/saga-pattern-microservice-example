using System.Threading.Tasks;

namespace ACME.Library.RabbitMq.Interfaces
{
    public interface IEventPublisher
    {
        Task CreateEventAsync<T>(T @event);
        Task CreateEventAsync<T>(T @event, string routingKey);
        Task TryPublishPendingEventsAsync();
    }
}