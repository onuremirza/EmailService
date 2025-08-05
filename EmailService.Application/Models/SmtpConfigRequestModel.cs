namespace EmailService.Application.Models;

public class SmtpConfigRequestModel
{
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public bool ForceTls { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string From { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public string UnsubscribeUrl { get; set; } = null!;
    public bool SupportsHtml { get; set; } = true;
    public string? Domain { get; set; }
    public string? DkimSelector { get; set; }
    public int Timeout { get; set; } = 10000;
    public int? RateLimitPerMinute { get; set; }
    public int MaxRetries { get; set; } = 3;
    public int Priority { get; set; } = 0;
    public string? CustomHeadersJson { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public List<SmtpHeaderRequestModel> Headers { get; set; } = [];
}
