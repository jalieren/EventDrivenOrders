using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace OrderAPI.Messaging;

public interface IRabbitMqService
{
    void PublishOrderCreated(object message);
}

public class RabbitMqService : IRabbitMqService
{
    private readonly ConnectionFactory _factory;

    public RabbitMqService()
    {
        _factory = new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };
    }
    
    public void PublishOrderCreated(object message)
    {
        // соединение
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        // создаём очередь
        channel.QueueDeclare(
            queue: "order.created.queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // публикуем
        channel.BasicPublish(
            exchange: "",
            routingKey: "order.created.queue",
            basicProperties: null,
            body: body);
    }
}