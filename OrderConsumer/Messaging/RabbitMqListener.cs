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
            
            var order = JsonSerializer.Deserialize<Order>(messageJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (order != null)
            {
                Console.WriteLine($"Получен заказ {order.Id} для {order.Email}, сумма {order.Amount}");
                
                // сохранение в бд
                try
                {
                    using var pgConn = new NpgsqlConnection(_pgConnectionString);
                    pgConn.Open();

                    // идемпотентность
                    var cmdCheck = new NpgsqlCommand("SELECT COUNT(*) FROM Orders WHERE IdempKey = @key", pgConn);
                    cmdCheck.Parameters.AddWithValue("key", order.IdempKey);
                    var exists = (long)cmdCheck.ExecuteScalar() > 0;

                    if (exists)
                    {
                        Console.WriteLine($"Заказ с IdempotencyKey {order.IdempKey} уже существует");
                        return;
                    }

                    // Сохраняем заказ
                    var cmdInsert = new NpgsqlCommand(
                        "INSERT INTO Orders (Id, Email, Amount, IdempKey, CreatedAt) VALUES (@id, @email, @amount, @key, @created)",
                        pgConn
                    );
                    cmdInsert.Parameters.AddWithValue("id", order.Id);
                    cmdInsert.Parameters.AddWithValue("email", order.Email);
                    cmdInsert.Parameters.AddWithValue("amount", order.Amount);
                    cmdInsert.Parameters.AddWithValue("key", order.IdempKey);
                    cmdInsert.Parameters.AddWithValue("created", order.CreatedAt);

                    cmdInsert.ExecuteNonQuery();
                    Console.WriteLine($"Заказ {order.Id} сохранён в базу");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при сохранении заказа {order.Id}: {ex.Message}");
                }
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