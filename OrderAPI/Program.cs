using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.OpenApi;
using OrderAPI.Contracts;
using OrderAPI.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order API",
        Version = "v1",
        Description = "API для создания заказов с RabbitMQ"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// включаем Swagger 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1");
        c.RoutePrefix = "";
    });
}

// app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok("OK"));

app.MapPost("/orders", (CreateOrderRequest request, ILogger<Program> logger, IRabbitMqService rabbitMqService) =>
    {
        // валидация и логи
        if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains("@"))
        {
            logger.LogWarning("Попытка создать заказ с некорректным Email: {Email}", request.CustomerEmail);
            return Results.BadRequest(new { Error = "Неверный email" });
        }

        if (request.Amount <= 0)
        {
            logger.LogWarning("Попытка создать заказ с отрицательной суммой: {Amount}", request.Amount);
            return Results.BadRequest(new { Error = "Сумма должна быть больше нуля" });
        }

        // генерация id
        var orderId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        
        // генерация ключа идемпотентности
        var requestJson = JsonSerializer.Serialize(request);
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(requestJson));
        var idempKey = Convert.ToBase64String(hashBytes);
        
        // сообщение для RabbitMQ
        var orderMessage = new
        {
            OrderId = orderId,
            CustomerEmail = request.CustomerEmail,
            Amount = request.Amount,
            IdempKey = idempKey,
            CreatedAt = createdAt
        };
        
        // логируем создание заказа
        logger.LogInformation("Создание заказа {OrderId} от {Email}, сумма {Amount}", 
            orderId, request.CustomerEmail, request.Amount);

        rabbitMqService.PublishOrderCreated(orderMessage);
        
        return Results.Ok(new { OrderId = orderId, IdempotencyKey = idempKey });
    }
);

app.Run();