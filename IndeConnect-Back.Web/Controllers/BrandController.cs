using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/brands")]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    /// <summary>
    /// Get brands sorted by ethics criteria
    /// </summary>
    /// <param name="sortBy">Ethics category to sort by</param>
    /// <param name="lat">User latitude (optional, for transport ethics)</param>
    /// <param name="lon">User longitude (optional, for transport ethics)</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BrandsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BrandsListResponse>> GetBrands(
        [FromQuery] EthicsSortType sortBy = EthicsSortType.MaterialsManufacturing,
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetBrandsQuery(sortBy, lat, lon, page, pageSize);
            var response = await _brandService.GetBrandsSortedByEthicsAsync(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}