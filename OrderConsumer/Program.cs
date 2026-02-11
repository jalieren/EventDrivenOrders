using OrderConsumer;
using OrderConsumer.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<RabbitMqListener>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();