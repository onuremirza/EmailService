using RabbitMQ.Client;

namespace EmailService.Infrastructure.Interfaces;

public interface IRabbitMqConnection : IDisposable
{
    void Connect();
    IModel CreateChannel();
}
