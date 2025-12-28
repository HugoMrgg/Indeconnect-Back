using IndeConnect_Back.Application.DTOs.Sizes;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class SizeService : ISizeService
{
    private readonly AppDbContext _context;

    public SizeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SizeLookupDto>> GetSizesByCategoryAsync(long categoryId)
    {
        var sizes = await _context.Sizes
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.SortOrder)
            .Select(s => new SizeLookupDto(
                s.Id,
                s.Name,
                s.CategoryId,
                s.SortOrder
            ))
            .ToListAsync();

        return sizes;
    }

    public async Task<IEnumerable<SizeLookupDto>> GetAllSizesAsync()
    {
        var sizes = await _context.Sizes
            .OrderBy(s => s.CategoryId)
            .ThenBy(s => s.SortOrder)
            .Select(s => new SizeLookupDto(
                s.Id,
                s.Name,
                s.CategoryId,
                s.SortOrder
            ))
            .ToListAsync();

        return sizes;
    }
}