using OrderAPI.Models;

namespace OrderAPI.Services;

public interface IOrderRepository
{
    Task<Order?> GetByIdempotencyKeyAsync(string idempKey);
    Task<Order> CreateAsync(Order order);
}

