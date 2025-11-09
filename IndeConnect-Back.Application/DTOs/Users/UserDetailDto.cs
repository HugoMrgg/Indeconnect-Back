namespace IndeConnect_Back.Application.DTOs.Users;

public record UserDetailDto(
    long Id,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset CreatedAt,
    bool IsEnabled,
    string RoleName,
    int BrandSubscriptionsCount,
    int ReviewsCount,
    int OrdersCount
);