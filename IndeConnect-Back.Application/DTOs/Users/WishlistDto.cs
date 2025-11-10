namespace IndeConnect_Back.Application.DTOs.Users;

public record WishlistDto(
    long Id,
    long UserId,
    IEnumerable<WishlistItemDto> Items,
    int TotalItems
);