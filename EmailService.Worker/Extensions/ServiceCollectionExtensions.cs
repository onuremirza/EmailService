using EmailService.Worker.Services;

namespace EmailService.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        _ = services.AddHostedService<EmailMessageConsumerService>();
        return services;
    }
}
