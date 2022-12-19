using System;

namespace ACME.Library.Common.Outbox
{
    public interface IOutboxMessage
    {
        string Topic { get; }
        DateTime TimeStamp { get; }
        OutboxMessageState State { get; }
        
        object RecreateObject();
    }
}