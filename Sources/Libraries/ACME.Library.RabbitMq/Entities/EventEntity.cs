namespace ACME.Library.RabbitMq.Entities
{
    public class EventEntity
    {
        public long Id { get; set; }
        public string Data { get; set; }
        public string RoutingKey { get; set; }
        public int RetryCount { get; set; }
    }
}