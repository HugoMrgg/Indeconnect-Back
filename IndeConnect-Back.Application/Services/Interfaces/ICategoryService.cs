using IndeConnect_Back.Application.DTOs.Categories;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryLookupDto>> GetAllCategoriesAsync();
}