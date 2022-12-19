using System.Threading.Tasks;

namespace ACME.Library.Common.Outbox
{
    public interface IOutboxPublisher
    {
        Task PublishMessagesAsync();
    }
}