using System;

namespace ACME.Library.RabbitMq.Configuration
{
    public class DefaultDeadLetterConfiguration : DeadLetterConfiguration
    {
        public DefaultDeadLetterConfiguration()
        {
            MaximumRetries = 3;
            RetryDelay = TimeSpan.FromMinutes(1);
        }
    }
}