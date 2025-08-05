namespace EmailService.Domain.Entities
{
    public class EmailMessageLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Recipient { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public string RabbitDeliveryTag { get; set; } = null!;
    }

}
