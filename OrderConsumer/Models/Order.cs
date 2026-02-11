using System;
using System.Text.Json.Serialization;

namespace OrderConsumer.Models;

public sealed class Order
{
    [JsonPropertyName("orderId")]
    public Guid Id { get; set; }

    [JsonPropertyName("customerEmail")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("idempKey")]
    public string IdempKey { get; set; } = null!;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}