using ACME.Library.Common.Outbox;
using System.Threading.Tasks;

namespace ACME.Library.Outbox
{
    public class TransactionalMessagePublisher : ITransactionalMessagePublisher
    {
        private readonly IOutboxRepository _outboxRepo;
        private readonly IOutboxMessageFactory _factory;
        private readonly IOutboxProcessor _processor;

        public TransactionalMessagePublisher(IOutboxRepository outboxRepo, IOutboxMessageFactory factory, IOutboxProcessor processor)
        {
            _outboxRepo = outboxRepo;
            _factory = factory;
            _processor = processor;
        }

        public async Task<IOutboxMessage> CreateMessageAsync(object data, string topic)
        {
            var outboxMessage = _factory.Create(data, topic);
            await _outboxRepo.AddMessageAsync(outboxMessage).ConfigureAwait(false);
            await _outboxRepo.SaveChangesAsync();
            return outboxMessage;
        }

        public void Publish()
        {
            _processor.ProcessOutbox();
        }
    }
}