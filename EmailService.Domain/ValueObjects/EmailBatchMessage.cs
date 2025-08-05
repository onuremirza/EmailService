namespace EmailService.Domain.ValueObjects;

public class EmailBatchMessage
{
    public required List<EmailRecipientMessage> Recipients { get; init; }
    public required string Subject { get; init; }
    public string? TemplateCode { get; init; }
    public Guid? SmtpConfigId { get; init; }
}
