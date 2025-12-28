using IndeConnect_Back.Application.DTOs.Sizes;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/sizes")]
public class SizeController : ControllerBase
{
    private readonly ISizeService _sizeService;

    public SizeController(ISizeService sizeService)
    {
        _sizeService = sizeService;
    }

    /// <summary>
    /// Get sizes by category ID
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<SizeLookupDto>>> GetSizesByCategory(long categoryId)
    {
        var sizes = await _sizeService.GetSizesByCategoryAsync(categoryId);
        return Ok(sizes);
    }

    /// <summary>
    /// Get all sizes (optional - utile pour debug)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SizeLookupDto>>> GetAllSizes()
    {
        var sizes = await _sizeService.GetAllSizesAsync();
        return Ok(sizes);
    }
}