using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using EmailService.Domain.ValueObjects;
using EmailService.Infrastructure.Interfaces;
using EmailService.Infrastructure.Persistence;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace EmailService.Worker.Services;

public class EmailMessageConsumerService : BackgroundService
{
    private const string QueueName = "email.send.queue";

    private readonly IRabbitMqConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailMessageConsumerService> _logger;
    private IModel? _channel;

    public EmailMessageConsumerService(
        IRabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailMessageConsumerService> logger)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateChannel();
        _ = _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        EventingBasicConsumer consumer = new(_channel);
        consumer.Received += async (_, args) =>
        {
            string json = Encoding.UTF8.GetString(args.Body.ToArray());
            EmailMessage? message = null;

            try
            {
                message = JsonSerializer.Deserialize<EmailMessage>(json);
                if (message == null)
                {
                    throw new Exception("Deserialization failed: message is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize message.");
                _channel.BasicNack(args.DeliveryTag, false, false);
                return;
            }

            using IServiceScope scope = _scopeFactory.CreateScope();
            IEmailProcessor processor = scope.ServiceProvider.GetRequiredService<IEmailProcessor>();
            EmailDbContext db = scope.ServiceProvider.GetRequiredService<EmailDbContext>();

            bool success = false;
            string? error = null;

            try
            {
                await processor.ProcessAsync(message);
                success = true;
                _channel.BasicAck(args.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                _logger.LogError(ex, "Email processing failed.");
                _channel.BasicNack(args.DeliveryTag, false, true);
            }

            _ = db.EmailMessageLogs.Add(new EmailMessageLog
            {
                Recipient = message.To,
                Subject = message.Subject,
                Success = success,
                ErrorMessage = error,
                RabbitDeliveryTag = args.DeliveryTag.ToString()
            });

            _ = await db.SaveChangesAsync();
        };

        _ = _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("Email consumer started.");
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _channel?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
