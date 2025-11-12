using IndeConnect_Back.Application.DTOs.Users;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IWishlistService
{
    Task<WishlistDto> GetUserWishlistAsync(long userId);
    Task<WishlistDto> AddProductToWishlistAsync(long userId, long productId);
    Task RemoveProductFromWishlistAsync(long userId, long productId);
    Task<bool> IsProductInWishlistAsync(long userId, long productId);
}