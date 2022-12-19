using System.Threading.Tasks;

namespace ACME.Library.Saga.Abstractions
{
    public interface IMessageHandler<in TMessage>
    {
        Task HandleAsync(TMessage message);
    }
}