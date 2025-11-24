namespace IndeConnect_Back.Application.DTOs.Users;

public record CartItemDto(
    long ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal
);