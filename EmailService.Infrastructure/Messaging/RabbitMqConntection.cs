using EmailService.Domain.Entities;
using EmailService.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EmailService.Infrastructure.Messaging;

public sealed class RabbitMqConnection : IRabbitMqConnection
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqConnection> _logger;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
    {
        _logger = logger;
        RabbitMqOptions cfg = options.Value;

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = cfg.Host,
            Port = cfg.Port,
            VirtualHost = cfg.VirtualHost,
            UserName = cfg.Username,
            Password = cfg.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _logger.LogInformation("RabbitMQ connected to {Host}:{Port}/{VHost}", cfg.Host, cfg.Port, cfg.VirtualHost);
    }

    public bool IsConnected => _connection.IsOpen;

    public IModel CreateChannel()
    {
        if (!IsConnected) throw new InvalidOperationException("RabbitMQ connection is not established.");
        IModel channel = _connection.CreateModel();
        return channel;
    }

    public void Dispose()
    {
        try { _connection.Close(); } catch { /* noop */ }
        _connection.Dispose();
    }
}
