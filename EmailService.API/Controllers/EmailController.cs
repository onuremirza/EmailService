using EmailService.Application.Interfaces;
using EmailService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmailService.API.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class EmailApiController : ControllerBase
{
    private readonly IEmailProcessor _processor;
    private readonly ILogger<EmailApiController> _logger;

    public EmailApiController(IEmailProcessor processor, ILogger<EmailApiController> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    [HttpPost]
    [EnableRateLimiting("EmailRateLimit")]
    public async Task<IActionResult> SendEmail([FromBody] EmailMessage message)
    {
        try
        {
            await _processor.ProcessAsync(message);
            return Ok(new { status = "sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send single email to {Recipient}", message.To);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    [EnableRateLimiting("EmailRateLimit")]
    public async Task<IActionResult> SendBatch([FromBody] EmailBatchMessage batch)
    {
        int successCount = 0;
        int failureCount = 0;
        List<string> failedRecipients = [];

        foreach (EmailRecipientMessage recipient in batch.Recipients)
        {
            try
            {
                EmailMessage message = new()
                {
                    To = recipient.To,
                    Subject = batch.Subject,
                    TemplateCode = batch.TemplateCode,
                    Params = recipient.Params,
                    UnsubscribeToken = recipient.UnsubscribeToken,
                    SmtpConfigId = batch.SmtpConfigId
                };

                await _processor.ProcessAsync(message);
                successCount++;
            }
            catch (Exception ex)
            {
                failureCount++;
                failedRecipients.Add(recipient.To);
                _logger.LogError(ex, "Failed to send email to {Recipient}", recipient.To);
            }
        }

        return Ok(new
        {
            status = "completed",
            total = batch.Recipients.Count,
            sent = successCount,
            failed = failureCount,
            failedRecipients
        });
    }
}
