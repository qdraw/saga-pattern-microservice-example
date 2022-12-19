using Newtonsoft.Json;
using ACME.Library.Common.Outbox;
using System;

namespace ACME.Library.Outbox.EntityFramework.Entities
{
    public class OutboxMessage : IOutboxMessage
    {
        public long Id { get; set; }
        public string Topic { get; set; }
        public DateTime TimeStamp { get; set; }
        public OutboxMessageState State { get; set; }
        public string Data { get; set; }

        public void ChangeState(OutboxMessageState newState)
        {
            State = newState;
        }

        public object RecreateObject()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            var data = JsonConvert.DeserializeObject(Data, settings);
            return data;
        }
    }
}