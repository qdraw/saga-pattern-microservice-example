using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ACME.Library.Common.Bus;
using ACME.Library.RabbitMq.Entities;
using ACME.Library.RabbitMq.Interfaces;
using System;
using System.Threading.Tasks;

namespace ACME.Library.RabbitMq.Services
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger<EventPublisher> logger;
        private readonly IMessageBus bus;
        private readonly IEventRepository eventRepository;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public EventPublisher(ILogger<EventPublisher> logger, IMessageBus bus,
            IEventRepository eventRepository)
        {
            this.logger = logger;
            this.bus = bus;
            this.eventRepository = eventRepository;

            jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public Task CreateEventAsync<T>(T @event)
        {
            return CreateEventAsync(@event, string.Empty);
        }

        public Task CreateEventAsync<T>(T @event, string routingKey)
        {
            var eventData = JsonConvert.SerializeObject(@event, jsonSerializerSettings);
            var entity = new EventEntity
            {
                Data = eventData,
                RoutingKey = routingKey
            };
            return eventRepository.CreateAsync(entity);
        }

        public async Task TryPublishPendingEventsAsync()
        {
            try
            {
                var pendingEvents = await eventRepository.GetPendingEvents();
                foreach (var pendingEvent in pendingEvents)
                {
                    await PublishEventAsync(pendingEvent);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to retrieve/update pending events");
            }
        }

        private async Task PublishEventAsync(EventEntity pendingEvent)
        {
            try
            {
                var @event = JsonConvert.DeserializeObject(pendingEvent.Data, jsonSerializerSettings);

                await bus.PublishAsync(@event, pendingEvent.RoutingKey);

                await eventRepository.RemoveAsync(pendingEvent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to publish event");
                pendingEvent.RetryCount++;
                await eventRepository.UpdateAsync(pendingEvent);
            }
        }
    }
}