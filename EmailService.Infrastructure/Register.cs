using EmailService.Domain.Entities;
using EmailService.Infrastructure.Email;
using EmailService.Infrastructure.Interfaces;
using EmailService.Infrastructure.Messaging;
using EmailService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Infrastructure;

public static class Register
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EmailDbContext>(o =>
            o.UseNpgsql(configuration.GetConnectionString("EmailDb")));

        services.Configure<SmtpConfig>(configuration.GetSection("Smtp"));
        services.Configure<RabbitMqConfig>(configuration.GetSection("RabbitMq"));


        services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

        return services;
    }
}
