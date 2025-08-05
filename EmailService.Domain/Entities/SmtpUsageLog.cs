namespace EmailService.Domain.Entities
{
    public class SmtpUsageLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SmtpConfigId { get; set; }
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
        public string Recipient { get; set; } = null!;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }

        public SmtpConfig SmtpConfig { get; set; } = null!;
    }

}
