// EmailService.Infrastructure/Email/EmailSender.cs  (CHANGED: UTF8 encodings + HTML detection)
using EmailService.Domain.Entities;
using EmailService.Domain.ValueObjects;
using EmailService.Infrastructure.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;                                  // CHANGED

namespace EmailService.Infrastructure.Email;

public class EmailSender : IEmailSender, IDisposable
{
    private readonly SmtpClient _client;
    private readonly string _from;
    private readonly string _fromName;
    private readonly string _unsubscribeUrl;
    private readonly IReadOnlyList<SmtpHeader> _headers;
    private readonly bool _supportsHtml;
    private readonly ITemplateRenderer _renderer;

    public EmailSender(IOptions<SmtpConfig> config, ITemplateRenderer renderer)
    {
        SmtpConfig cfg = config.Value;

        _client = new SmtpClient(cfg.Host, cfg.Port)
        {
            EnableSsl = cfg.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(cfg.Username, cfg.Password),
            Timeout = cfg.Timeout
        };

        _from = cfg.From;
        _fromName = cfg.FromName;
        _unsubscribeUrl = cfg.UnsubscribeUrl;
        _headers = cfg.Headers.AsReadOnly();
        _supportsHtml = cfg.SupportsHtml;
        _renderer = renderer;
    }

    public async Task SendAsync(EmailMessage message, string body, string subject)
    {
        using MailMessage mail = new()
        {
            From = new MailAddress(_from, _fromName),
            Subject = subject,
            Body = body,
            SubjectEncoding = Encoding.UTF8,         // CHANGED
            BodyEncoding = Encoding.UTF8,            // CHANGED
            IsBodyHtml = _supportsHtml || LooksLikeHtml(body) // CHANGED
        };

        mail.To.Add(message.To);

        foreach (SmtpHeader header in _headers)
        {
            if (!string.IsNullOrWhiteSpace(header.Key) && !string.IsNullOrWhiteSpace(header.Value))
            {
                mail.Headers[header.Key] = header.Value;
            }
        }

        await _client.SendMailAsync(mail);
    }

    private static bool LooksLikeHtml(string s)
    {
        // CHANGED
        return !string.IsNullOrWhiteSpace(s) && s.Contains('<') && s.Contains('>'); // CHANGED
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
