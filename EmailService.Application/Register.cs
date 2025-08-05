using EmailService.Application.Interfaces;
using EmailService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Application;

public static class Register
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        _ = services.AddScoped<IEmailProcessor, EmailProcessor>();
        _ = services.AddScoped<IEmailQueuePublisher, EmailQueuePublisher>();
        _ = services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        _ = services.AddScoped<ISmtpConfigService, SmtpConfigService>();

        return services;
    }
}
