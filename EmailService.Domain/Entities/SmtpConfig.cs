namespace EmailService.Domain.Entities;

public class SmtpConfig
{
    public Guid Id { get; set; }
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public bool ForceTls { get; set; } = false;
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

    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsedAt { get; set; }

    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }

    public List<SmtpHeader> Headers { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
