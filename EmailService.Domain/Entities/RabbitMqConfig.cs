namespace EmailService.Domain.Entities;

public class RabbitMqConfig
{
    public Guid Id { get; set; }
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Exchange { get; set; } = "email.exchange";
    public string QueueName { get; set; } = "email.send.queue";
    public string RoutingKey { get; set; } = "email.send";
    public bool IsActive { get; set; } = true;
}