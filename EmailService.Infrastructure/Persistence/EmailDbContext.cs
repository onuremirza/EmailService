using EmailService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailService.Infrastructure.Persistence;

public class EmailDbContext : DbContext
{
    public EmailDbContext(DbContextOptions<EmailDbContext> options) : base(options) { }

    public DbSet<EmailMessageLog> EmailMessageLogs => Set<EmailMessageLog>();
    public DbSet<SmtpConfig> SmtpConfigs => Set<SmtpConfig>();
    public DbSet<SmtpHeader> SmtpHeaders => Set<SmtpHeader>();
    public DbSet<SmtpUsageLog> SmtpUsageLogs => Set<SmtpUsageLog>();
    public DbSet<RabbitMqConfig> RabbitMqConfigs => Set<RabbitMqConfig>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

}
