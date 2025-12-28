using IndeConnect_Back.Application.DTOs.Sizes;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface ISizeService
{
    Task<IEnumerable<SizeLookupDto>> GetSizesByCategoryAsync(long categoryId);
    Task<IEnumerable<SizeLookupDto>> GetAllSizesAsync();
}