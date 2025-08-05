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
        _ = services.AddDbContext<EmailDbContext>(options =>
        {
            _ = options.UseNpgsql(configuration.GetConnectionString("EmailDB"));
        });

        _ = services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
        _ = services.AddScoped<IEmailSender, EmailSender>();
        _ = services.AddScoped<IRabbitMqConnection, RabbitMqConnection>();

        return services;
    }
}
