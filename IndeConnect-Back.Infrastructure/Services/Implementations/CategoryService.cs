using IndeConnect_Back.Application.DTOs.Categories;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryLookupDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryLookupDto(c.Id, c.Name))
            .ToListAsync();

        return categories;
    }
}