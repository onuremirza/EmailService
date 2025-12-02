namespace EmailService.Domain.Entities
{
    public sealed class RabbitMqOptions
    {
        public Guid Id { get; set; }
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Exchange { get; set; } = "email.direct.exchange";
        public string ExchangeType { get; set; } = "direct";
        public string Queue { get; set; } = "email.send.queue";
        public string RoutingKey { get; set; } = "email.send";
        public string DeadLetterExchange { get; set; } = "email.dlx";
        public string DeadLetterQueue { get; set; } = "email.dead.queue";
        public string DeadLetterRoutingKey { get; set; } = "email.dead";
        public ushort PrefetchCount { get; set; } = 32;
        public bool PublisherConfirms { get; set; } = true;
        public bool PersistentMessages { get; set; } = true;

        public RetryOptions Retry { get; set; } = new();
        public bool IsActive { get; set; }

        public sealed class RetryOptions
        {
            public Guid Id { get; set; }
            public bool Enabled { get; set; } = true;
            public int MaxAttempts { get; set; } = 5;
            public int InitialDelaySeconds { get; set; } = 5;
            public int MaxDelaySeconds { get; set; } = 300;
        }
    }

}
