using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using EmailService.Domain.ValueObjects;
using EmailService.Infrastructure.Email;
using EmailService.Infrastructure.Interfaces;
using EmailService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class EmailProcessor : IEmailProcessor
{
    private readonly EmailDbContext _db;
    private readonly ILogger<EmailProcessor> _logger;
    private readonly ITemplateRenderer _renderer;

    public EmailProcessor(EmailDbContext db, ILogger<EmailProcessor> logger, ITemplateRenderer renderer)
    {
        _db = db;
        _logger = logger;
        _renderer = renderer;
    }

    public async Task ProcessAsync(EmailMessage message)
    {
        SmtpConfig? smtp = null;

        if (message.SmtpConfigId.HasValue)
        {
            smtp = await _db.SmtpConfigs
                .Include(x => x.Headers)
                .FirstOrDefaultAsync(x => x.Id == message.SmtpConfigId && x.IsActive);
        }

        smtp ??= await _db.SmtpConfigs
            .Include(x => x.Headers)
            .FirstOrDefaultAsync(x => x.IsDefault && x.IsActive);

        if (smtp == null)
        {
            throw new InvalidOperationException("No SMTP configuration found.");
        }

        string subject;
        string body;

        if (!string.IsNullOrWhiteSpace(message.TemplateCode))
        {
            EmailTemplate? template = await _db.EmailTemplates
                .FirstOrDefaultAsync(t => t.Code == message.TemplateCode && t.IsActive);

            if (template == null)
            {
                throw new InvalidOperationException("Template not found or inactive.");
            }

            body = _renderer.Render(template.Body, message.Params ?? [], message.UnsubscribeToken);
            subject = _renderer.Render(
                !string.IsNullOrWhiteSpace(message.Subject) ? message.Subject : template.Subject,
                message.Params ?? [],
                null,
                appendUnsubscribe: false
            );
        }
        else
        {
            if (string.IsNullOrWhiteSpace(message.Subject))
            {
                throw new InvalidOperationException("Subject is required when no template is provided.");
            }

            body = "";
            subject = _renderer.Render(
                message.Subject,
                message.Params ?? [],
                message.UnsubscribeToken
            );
        }

        bool success = false;
        string? error = null;

        try
        {
            using EmailSender sender = new(Options.Create(smtp), _renderer);
            await sender.SendAsync(message, body, subject);
            success = true;
            _logger.LogInformation("Email sent to {Recipient}", message.To);
        }
        catch (Exception ex)
        {
            error = ex.Message;
            _logger.LogError(ex, "Failed to send email to {Recipient}", message.To);
        }

        _ = _db.SmtpUsageLogs.Add(new SmtpUsageLog
        {
            SmtpConfigId = smtp.Id,
            Recipient = message.To,
            Success = success,
            ErrorMessage = error
        });

        smtp.UsageCount++;
        smtp.LastUsedAt = DateTime.UtcNow;

        _ = await _db.SaveChangesAsync();
    }
}
