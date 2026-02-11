using OrderConsumer;
using OrderConsumer.Messaging;

var builder = Host.CreateApplicationBuilder(args);

var pgConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=orders";

builder.Services.AddSingleton(new RabbitMqListener(pgConnectionString));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();