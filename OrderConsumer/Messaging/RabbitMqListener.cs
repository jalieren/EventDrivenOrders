using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using OrderConsumer.Models;
using Npgsql;

namespace OrderConsumer.Messaging;

public class RabbitMqListener
{
    private readonly string _hostName = "localhost";
    private readonly string _queueName = "order.created.queue";
    private readonly string _pgConnectionString;
    
    public RabbitMqListener(string pgConnectionString)
    {
        _pgConnectionString = pgConnectionString;
    }
    
    public void StartListening()
    {
        // подключение
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            UserName = "guest",
            Password = "guest"
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);
            var order = JsonSerializer.Deserialize<Order>(messageJson);

            if (order != null)
            {
                Console.WriteLine($"Получен заказ {order.Id} для {order.Email}, сумма {order.Amount}");
                
                // дописать логику сохранения в бд
            }
        };

        channel.BasicConsume(
            queue: _queueName,
            autoAck: true,
            consumer: consumer
        );

        Console.WriteLine("Listener запущен. Ожидание сообщений");
    }
}