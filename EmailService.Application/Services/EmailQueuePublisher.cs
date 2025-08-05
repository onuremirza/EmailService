using EmailService.Application.Interfaces;
using EmailService.Domain.ValueObjects;
using EmailService.Infrastructure.Messaging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace EmailService.Application.Services;

public class EmailQueuePublisher : IEmailQueuePublisher
{
    private readonly IModel _channel;
    private readonly string _queueName = "email.send.queue";

    public EmailQueuePublisher(RabbitMqConnection connection)
    {
        _channel = connection.CreateChannel();

        _ = _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public Task PublishBatchAsync(EmailBatchMessage batch)
    {
        IBasicProperties properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        foreach (var recipient in batch.Recipients)
        {
            var message = new EmailMessage
            {
                To = recipient.To,
                Subject = batch.Subject,
                TemplateCode = batch.TemplateCode,
                Params = recipient.Params,
                UnsubscribeToken = recipient.UnsubscribeToken,
                SmtpConfigId = batch.SmtpConfigId
            };

            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: properties,
                body: body
            );
        }

        return Task.CompletedTask;
    }
}
