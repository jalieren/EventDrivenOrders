using Microsoft.Extensions.Hosting;
using OrderConsumer.Messaging;
namespace OrderConsumer;

public class Worker : BackgroundService
{
    private readonly RabbitMqListener _listener;
    private readonly ILogger<Worker> _logger;
    
    public Worker(RabbitMqListener listener, ILogger<Worker> logger)
    {
        _listener = listener;
        _logger = logger;
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // запускаем listener
        _listener.StartListening();

        // держим worker живым
        _logger.LogInformation("Worker запущен. Ожидание сообщений");
        return Task.CompletedTask;
    }
}