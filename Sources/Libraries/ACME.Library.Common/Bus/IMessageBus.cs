namespace ACME.Library.Common.Bus
{
    using System;
    using System.Threading.Tasks;

    public interface IMessageBus
    {
        Task PublishAsync<TMessage>(TMessage message);
        Task PublishAsync<TMessage>(TMessage message, string topic);
        Task ConsumeAsync<TMessage>(Func<TMessage, Task> delegateAction);
        Task ConsumeAsync<TMessage>(Func<TMessage, Task> delegateAction, ushort prefetchCount);
        Task ConsumeAsync<TMessage>(Func<TMessage, Task> delegateAction, string topic);
        Task ConsumeAsync<TMessage>(Func<TMessage, Task> delegateAction, string topic, ushort prefetchCount);
        Task<TResponse> RequestAsync<TRequest, TResponse>(TRequest request);
        Task RespondAsync<TRequest, TResponse>(Func<TRequest, Task<TResponse>> delegateAction);
    }
}