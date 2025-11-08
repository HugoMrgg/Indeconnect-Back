namespace IndeConnect_Back.Application.DTOs.Subscriptions;


public record BrandSubscriptionItem(
    long BrandId,
    string BrandName,
    string? BrandLogoUrl,
    DateTimeOffset SubscribedAt
);