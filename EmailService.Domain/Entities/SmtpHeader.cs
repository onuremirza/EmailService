namespace EmailService.Domain.Entities;

public class SmtpHeader
{
    public Guid Id { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;

    public Guid SmtpConfigId { get; set; }
    public SmtpConfig SmtpConfig { get; set; } = null!;
}
