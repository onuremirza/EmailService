using EmailService.Application.Interfaces;
using EmailService.Domain.Entities;
using EmailService.Domain.ValueObjects;
using EmailService.Infrastructure.Interfaces;
using EmailService.Infrastructure.Persistence;
using EmailService.Worker.Health;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace EmailService.Worker.Services;

public sealed class EmailMessageConsumerService : BackgroundService
{
    private const string Exchange = "email.direct.exchange";
    private const string RoutingKey = "email.send";
    private const string QueueName = "email.send.queue";

    private readonly IRabbitMqConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailMessageConsumerService> _logger;
    private readonly ConsumerHealthState _state;

    private IModel? _channel;
    private string? _consumerTag;
    private CancellationToken _stoppingToken;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    public EmailMessageConsumerService(
        IRabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailMessageConsumerService> logger,
        ConsumerHealthState state)
    {
        _connection = connection;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _state = state;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        try
        {
            _channel = _connection.CreateChannel();

            _channel.BasicQos(0, 32, global: false);
            _channel.ExchangeDeclare(Exchange, ExchangeType.Direct, durable: true, autoDelete: false);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(QueueName, Exchange, RoutingKey);

            _channel.CallbackException += (_, ea) =>
            {
                _state.MarkError(ea.Exception?.Message ?? "CallbackException");
                _state.MarkConsuming(false);
            };

            _channel.ModelShutdown += (_, ea) =>
            {
                _state.MarkError($"ModelShutdown: {ea.ReplyText}");
                _state.MarkConsuming(false);
            };

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += OnReceivedAsync;

            _consumerTag = _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            _state.MarkStarted();
            _state.MarkConsuming(true);

            _logger.LogInformation(
                "Email consumer started on {Exchange}/{Queue} with {RoutingKey}. consumerTag={ConsumerTag}",
                Exchange, QueueName, RoutingKey, _consumerTag);

            // Worker'ı canlı tut
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // normal shutdown
        }
        catch (Exception ex)
        {
            _state.MarkError(ex.Message);
            _state.MarkConsuming(false);
            _logger.LogError(ex, "Consumer crashed during startup/run loop.");
            throw;
        }
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        IModel? channel = _channel;
        if (channel is null || !channel.IsOpen) return;

        _state.MarkMessage();

        string json = Encoding.UTF8.GetString(args.Body.ToArray());
        EmailMessage message;

        try
        {
            message = JsonSerializer.Deserialize<EmailMessage>(json, JsonOptions)
                      ?? throw new InvalidOperationException("Deserialization failed.");
        }
        catch (Exception ex)
        {
            _state.MarkError($"Deserialize: {ex.Message}");
            _logger.LogError(ex, "Invalid message. Rejecting without requeue.");
            SafeReject(channel, args.DeliveryTag, requeue: false);
            return;
        }

        bool success = false;
        string? error = null;

        try
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IEmailProcessor processor = scope.ServiceProvider.GetRequiredService<IEmailProcessor>();

            await processor.ProcessAsync(message);
            success = true;

            SafeAck(channel, args.DeliveryTag);
        }
        catch (Exception ex)
        {
            success = false;
            error = ex.Message;
            _state.MarkError($"Process: {ex.Message}");
            _logger.LogError(ex, "Email processing failed. Nack with requeue=true");
            SafeNack(channel, args.DeliveryTag, requeue: true);
        }

        try
        {
            using IServiceScope scope2 = _scopeFactory.CreateScope();
            EmailDbContext db = scope2.ServiceProvider.GetRequiredService<EmailDbContext>();

            db.EmailMessageLogs.Add(new EmailMessageLog
            {
                Recipient = message.To,
                Subject = message.Subject,
                Success = success,
                ErrorMessage = error,
                RabbitDeliveryTag = args.DeliveryTag.ToString()
            });

            await db.SaveChangesAsync(_stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown sırasında normal
        }
        catch (Exception ex)
        {
            _state.MarkError($"LogWrite: {ex.Message}");
            _logger.LogError(ex, "Failed to write EmailMessageLog.");
            // NOT: log yazılamadı diye mesajı requeue etmiyoruz (sonsuz döngüye girer)
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _state.MarkConsuming(false);
        _state.MarkError("Stopping");

        try
        {
            if (_channel is not null && _channel.IsOpen && !string.IsNullOrWhiteSpace(_consumerTag))
                _channel.BasicCancel(_consumerTag);
        }
        catch { /* swallow */ }

        try
        {
            _channel?.Close();
            _channel?.Dispose();
        }
        catch { /* swallow */ }

        return base.StopAsync(cancellationToken);
    }

    private static void SafeAck(IModel channel, ulong deliveryTag)
    {
        try { if (channel.IsOpen) channel.BasicAck(deliveryTag, multiple: false); } catch { }
    }

    private static void SafeNack(IModel channel, ulong deliveryTag, bool requeue)
    {
        try { if (channel.IsOpen) channel.BasicNack(deliveryTag, multiple: false, requeue: requeue); } catch { }
    }

    private static void SafeReject(IModel channel, ulong deliveryTag, bool requeue)
    {
        try { if (channel.IsOpen) channel.BasicReject(deliveryTag, requeue: requeue); } catch { }
    }
}
