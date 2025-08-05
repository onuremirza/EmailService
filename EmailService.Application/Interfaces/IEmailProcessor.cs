using EmailService.Domain.ValueObjects;

namespace EmailService.Application.Interfaces
{
    public interface IEmailProcessor
    {
        Task ProcessAsync(EmailMessage message);
    }
}
