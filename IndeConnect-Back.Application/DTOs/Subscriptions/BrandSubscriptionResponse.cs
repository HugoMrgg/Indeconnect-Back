namespace IndeConnect_Back.Application.DTOs.Subscriptions;

public record BrandSubscriptionResponse(
    long Id,
    long UserId,
    long BrandId,
    string BrandName,
    DateTimeOffset SubscribedAt
);