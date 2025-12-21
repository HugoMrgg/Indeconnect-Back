using IndeConnect_Back.Application.DTOs.Colors;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/colors")]
public class ColorController : ControllerBase
{
    private readonly IColorService _colorService;

    public ColorController(IColorService colorService)
    {
        _colorService = colorService;
    }

    /// <summary>
    /// Get all available colors
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ColorDto>>> GetColors()
    {
        var colors = await _colorService.GetAllColorsAsync();
        return Ok(colors);
    }
}