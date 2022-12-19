using Newtonsoft.Json;
using ACME.Library.Common.Outbox;
using System;
using ACME.Library.Outbox.EntityFramework.Entities;

namespace ACME.Library.Outbox.EntityFramework
{
    internal class OutboxMessageFactory : IOutboxMessageFactory
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public OutboxMessageFactory()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public IOutboxMessage Create(object data, string topic = default)
        {
            var serializedData = JsonConvert.SerializeObject(data, _serializerSettings);

            return new OutboxMessage
            {
                State = OutboxMessageState.Ready,
                TimeStamp = DateTime.UtcNow,
                Topic = topic,
                Data = serializedData
            };
        }
    }
}
