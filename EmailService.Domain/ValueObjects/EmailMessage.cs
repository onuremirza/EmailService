namespace EmailService.Domain.ValueObjects;

public class EmailMessage
{
    public string To { get; set; } = null!;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? TemplateCode { get; set; }
    public Dictionary<string, string>? Params { get; set; }
    public string? UnsubscribeToken { get; set; }
    public Guid? SmtpConfigId { get; set; }
}