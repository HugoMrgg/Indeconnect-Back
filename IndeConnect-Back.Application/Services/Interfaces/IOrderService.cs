using IndeConnect_Back.Application.DTOs.Orders;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(long userId, CreateOrderRequest request);
    Task<OrderDto?> GetOrderByIdAsync(long orderId);
    Task<List<OrderDto>> GetUserOrdersAsync(long userId);
}