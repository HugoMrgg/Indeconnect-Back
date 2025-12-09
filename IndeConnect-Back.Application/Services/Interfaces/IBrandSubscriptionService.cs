using IndeConnect_Back.Application.DTOs.Subscriptions;

namespace IndeConnect_Back.Application.Services.Interfaces;


public interface IBrandSubscriptionService
{
    Task<BrandSubscriptionResponse> SubscribeToBrandAsync(long? userId, long brandId);
    Task<UserBrandSubscriptionsResponse> GetUserSubscriptionsAsync(long? userId);
    Task UnsubscribeFromBrandAsync(long? userId, long brandId);
}