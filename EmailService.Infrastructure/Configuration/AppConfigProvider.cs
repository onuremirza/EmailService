using EmailService.Domain.Entities;
using EmailService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EmailService.Infrastructure.Configuration;

public class AppConfigProvider : IAppConfigProvider
{
    private readonly EmailDbContext _db;
    private readonly IMemoryCache _cache;

    public AppConfigProvider(EmailDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public Task<SmtpConfig> GetSmtpSettingsAsync()
    {
        return _cache.GetOrCreateAsync("smtp", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                SmtpConfig entity = await _db.SmtpConfigs.FirstAsync(x => x.IsActive);
                return new SmtpConfig
                {
                    Host = entity.Host,
                    Port = entity.Port,
                    Username = entity.Username,
                    Password = entity.Password,
                    EnableSsl = entity.EnableSsl,
                    From = entity.From,
                    UnsubscribeUrl = entity.UnsubscribeUrl
                };
            });
    }

    public Task<RabbitMqConfig> GetRabbitMqSettingsAsync()
    {
        return _cache.GetOrCreateAsync("rabbitmq", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                RabbitMqConfig entity = await _db.RabbitMqConfigs.FirstAsync(x => x.IsActive);
                return new RabbitMqConfig
                {
                    Host = entity.Host,
                    Port = entity.Port,
                    Username = entity.Username,
                    Password = entity.Password,
                    Exchange = entity.Exchange,
                    QueueName = entity.QueueName,
                    RoutingKey = entity.RoutingKey
                };
            });
    }
}