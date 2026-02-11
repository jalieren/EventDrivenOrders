
using OrderAPI.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok("OK"));

app.MapPost("/orders", (CreateOrderRequest request, ILogger<Program> logger) =>
    {
        // валидация
        if (string.IsNullOrWhiteSpace(request.CustomerEmail) || !request.CustomerEmail.Contains("@"))
            return Results.BadRequest(new { Error = "Неверный email" });

        if (request.Amount <= 0) 
            return Results.BadRequest(new { Error = "Сумма должна быть больше нуля" });
        
        // логируем получение
        logger.LogInformation("Получен новый заказ от {Email} на сумму {Amount}", 
            request.CustomerEmail, request.Amount);
        
        // генерация id
        var orderId = Guid.NewGuid();
        
        // логируем генерацию id
        logger.LogInformation("Сгенерирован ID заказа {orderID} от {Email}", 
            orderId, request.CustomerEmail);

        return Results.Ok(new { OrderId = orderId });
    }
);

app.Run();