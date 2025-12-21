using IndeConnect_Back.Application.DTOs.Colors;

namespace IndeConnect_Back.Application.Services.Interfaces;

public interface IColorService
{
    Task<IEnumerable<ColorDto>> GetAllColorsAsync();
}
