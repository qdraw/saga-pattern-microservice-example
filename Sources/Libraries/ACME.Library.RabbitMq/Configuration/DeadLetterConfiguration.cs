using System;

namespace ACME.Library.RabbitMq.Configuration
{
    public class DeadLetterConfiguration
    {
        public int MaximumRetries { get; set; }
        public TimeSpan RetryDelay { get; set; }
    }
}