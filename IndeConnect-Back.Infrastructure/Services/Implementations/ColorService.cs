using IndeConnect_Back.Application.DTOs.Colors;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain;
using Microsoft.EntityFrameworkCore;

namespace IndeConnect_Back.Infrastructure.Services.Implementations;

public class ColorService : IColorService
{
    private readonly AppDbContext _context;

    public ColorService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ColorDto>> GetAllColorsAsync()
    {
        var colors = await _context.Colors
            .OrderBy(c => c.Name)
            .Select(c => new ColorDto(c.Id, c.Name, c.Hexa))
            .ToListAsync();

        return colors;
    }
}
