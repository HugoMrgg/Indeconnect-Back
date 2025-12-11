namespace IndeConnect_Back.Application.DTOs.Subscriptions;

public record UserBrandSubscriptionsResponse(
    long? UserId,
    IEnumerable<BrandSubscriptionItem> Subscriptions
);