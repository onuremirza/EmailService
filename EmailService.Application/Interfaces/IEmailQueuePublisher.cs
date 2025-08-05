using EmailService.Domain.ValueObjects;

namespace EmailService.Application.Interfaces
{
    public interface IEmailQueuePublisher
    {
        Task PublishBatchAsync(EmailBatchMessage batch);
    }
}
