using RabbitMQ.Client;

namespace EmailService.Infrastructure.Interfaces;

public interface IRabbitMqConnection : IDisposable
{
    IModel CreateChannel();
}
