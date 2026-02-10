namespace Order.API.Contracts;

public sealed class CreateOrderRequest
{
    public string CustomerEmail { get; init; } = null!;
    public decimal Amount { get; init; }
}