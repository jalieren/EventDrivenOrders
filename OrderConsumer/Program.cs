using OrderConsumer;
using OrderConsumer.Messaging;

var builder = Host.CreateApplicationBuilder(args);

// строка подключения
var pgConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
                   ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSingleton<RabbitMqListener>(sp =>
    new RabbitMqListener(pgConnection));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();