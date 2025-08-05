using EmailService.Domain.Entities;
using EmailService.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EmailService.Infrastructure.Messaging;
public class RabbitMqConnection : IRabbitMqConnection
{
    private IConnection? _connection;
    private readonly ConnectionFactory _factory;
    private readonly ILogger<RabbitMqConnection> _logger;

    public RabbitMqConnection(
        IOptions<RabbitMqConfig> options,
        ILogger<RabbitMqConnection> logger)
    {
        _logger = logger;
        _factory = new ConnectionFactory
        {
            HostName = options.Value.Host,
            Port = options.Value.Port,
            UserName = options.Value.Username,
            Password = options.Value.Password
        };
    }

    public void Connect()
    {
        _connection ??= _factory.CreateConnection("EmailService");
        _logger.LogInformation("RabbitMQ connected to {Host}:{Port}", _factory.HostName, _factory.Port);
    }

    public IModel CreateChannel()
    {
        return _connection is null || !_connection.IsOpen
            ? throw new InvalidOperationException("RabbitMQ connection is not established.")
            : _connection.CreateModel();
    }

    public void Dispose()
    {
        try
        {
            if (_connection?.IsOpen == true)
            {
                _connection.Close();
            }

            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while disposing RabbitMQ connection.");
        }
    }
}
