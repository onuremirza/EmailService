using EmailService.Domain.ValueObjects;

namespace EmailService.Infrastructure.Interfaces;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, string body, string subject);
}
