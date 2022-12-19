using System.Threading.Tasks;
using ACME.Library.Common.Bus;
using ACME.Library.Common.Outbox;

namespace ACME.Library.Outbox
{
    public class OutboxPublisher : IOutboxPublisher
    {
        private readonly IOutboxRepository _outboxRepository;
        private readonly IMessageBus _bus;

        public OutboxPublisher(IOutboxRepository outboxRepository, IMessageBus bus)
        {
            _outboxRepository = outboxRepository;
            _bus = bus;
        }

        public async Task PublishMessagesAsync()
        {
            var messages = await _outboxRepository.GetUnpublishedMessagesAsync().ConfigureAwait(false);

            foreach (var message in messages)
            {
                var messageObject = message.RecreateObject();

                if (string.IsNullOrEmpty(message.Topic))
                {
                    await _bus.PublishAsync(messageObject);
                }
                else
                {
                    await _bus.PublishAsync(messageObject, message.Topic);
                }

                _outboxRepository.UpdateMessageState(message, OutboxMessageState.Shipped);

                await _outboxRepository.SaveChangesAsync();
            }
        }
    }
}