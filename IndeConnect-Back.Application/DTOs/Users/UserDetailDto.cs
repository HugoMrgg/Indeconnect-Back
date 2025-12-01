using IndeConnect_Back.Domain.user;

namespace IndeConnect_Back.Application.DTOs.Users;

public record UserDetailDto(
    long Id,
    string Email,
    string FirstName,
    string LastName,
    DateTimeOffset CreatedAt,
    bool IsEnabled,
    Role role,
    int BrandSubscriptionsCount,
    int ReviewsCount,
    int OrdersCount
);