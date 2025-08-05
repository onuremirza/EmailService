using EmailService.Domain.Entities;
using EmailService.Domain.ValueObjects;
using EmailService.Infrastructure.Interfaces;
using System.Net;
using System.Net.Mail;

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

    public EmailSender(SmtpConfig config, ITemplateRenderer renderer)
    {
        _client = new SmtpClient(config.Host, config.Port)
        {
            EnableSsl = config.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(config.Username, config.Password),
            Timeout = config.Timeout
        };

        _from = config.From;
        _fromName = config.FromName;
        _unsubscribeUrl = config.UnsubscribeUrl;
        _headers = config.Headers.AsReadOnly();
        _supportsHtml = config.SupportsHtml;
        _renderer = renderer;
    }

    public async Task SendAsync(EmailMessage message, string body, string subject)
    {
        using MailMessage mail = new()
        {
            From = new MailAddress(_from, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = _supportsHtml
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

    public void Dispose()
    {
        _client.Dispose();
    }
}
