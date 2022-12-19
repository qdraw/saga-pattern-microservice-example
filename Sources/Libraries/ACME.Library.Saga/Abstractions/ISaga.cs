namespace ACME.Library.Saga.Abstractions
{
    internal interface ISaga
    {
        void Initialize<TMessage>(TMessage message)
            where TMessage : class;
    }
}