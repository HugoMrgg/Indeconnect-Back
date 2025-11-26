namespace IndeConnect_Back.Application.DTOs.Users;

public record CartDto(
    long Id,
    long UserId,
    IEnumerable<CartItemDto> Items,
    int TotalItems,
    decimal TotalAmount
);