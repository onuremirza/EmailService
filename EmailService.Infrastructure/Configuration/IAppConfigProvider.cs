using EmailService.Domain.Entities;

namespace EmailService.Infrastructure.Configuration;

public interface IAppConfigProvider
{
    Task<SmtpConfig> GetSmtpSettingsAsync();
    Task<RabbitMqOptions> GetRabbitMqSettingsAsync();
}
