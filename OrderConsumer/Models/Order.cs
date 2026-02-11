namespace OrderConsumer.Models;

public sealed class Order
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public decimal Amount { get; set; }
    public string IdempKey { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}