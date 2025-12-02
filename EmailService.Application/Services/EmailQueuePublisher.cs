using EmailService.Application.Interfaces;
using EmailService.Domain.ValueObjects;
using EmailService.Infrastructure.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EmailService.Application.Services;

public sealed class EmailQueuePublisher : IEmailQueuePublisher
{
    private readonly IRabbitMqConnection _connection;
    private const string Exchange = "email";
    private const string QueueName = "email.send.queue";
    private const string RoutingKey = "email.send";

    public EmailQueuePublisher(IRabbitMqConnection connection)
    {
        _connection = connection;
    }

    public Task PublishBatchAsync(EmailBatchMessage batch)
    {
        using IModel channel = _connection.CreateChannel();

        channel.ExchangeDeclare(Exchange, ExchangeType.Direct, durable: true, autoDelete: false, arguments: null);
        channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueBind(QueueName, Exchange, RoutingKey);

        IBasicProperties props = channel.CreateBasicProperties();
        props.Persistent = true;

        foreach (EmailRecipientMessage recipient in batch.Recipients)
        {
            EmailMessage message = new EmailMessage
            {
                To = recipient.To,
                Subject = batch.Subject,
                TemplateCode = batch.TemplateCode,
                Params = recipient.Params,
                UnsubscribeToken = recipient.UnsubscribeToken,
                SmtpConfigId = batch.SmtpConfigId
            };

            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            channel.BasicPublish(Exchange, RoutingKey, props, body);
        }

        return Task.CompletedTask;
    }
}
