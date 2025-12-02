using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.DTOs.Products; 
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/brands")]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;
    private readonly IProductService _productService; 

    public BrandController(IBrandService brandService, IProductService productService) 
    {
        _brandService = brandService;
        _productService = productService; 
    }

    /**
     * Get brands sorted by ethics criteria
     */
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BrandsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BrandsListResponse>> GetBrands(
        [FromQuery] EthicsSortType sortBy = EthicsSortType.MaterialsManufacturing,
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? priceRange = null,
        [FromQuery] double? userRatingMin = null,
        [FromQuery] double? maxDistanceKm = 80,
        [FromQuery] double? minEthicsProduction = null,
        [FromQuery] double? minEthicsTransport = null,
        [FromQuery] string[]? ethicTags = null)  
    {
        var query = new GetBrandsQuery(
            sortBy, 
            lat, 
            lon, 
            page, 
            pageSize, 
            priceRange, 
            userRatingMin, 
            maxDistanceKm,
            minEthicsProduction,
            minEthicsTransport,
            ethicTags  
        );
        var response = await _brandService.GetBrandsSortedByEthicsAsync(query);
        return Ok(response);
    }
    /**
     * Get detailed brand information (presentation page)
     */
    [HttpGet("{brandId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BrandDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandDetailDto>> GetBrandById(
        [FromRoute] long brandId,
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null)
    {
        var brand = await _brandService.GetBrandByIdAsync(brandId, lat, lon);
        
        if (brand == null)
            return NotFound(new { message = "Brand not found" });

        return Ok(brand);
    }
    
    /**
     * Get products of a specific brand with filters (products page)
     */
    [HttpGet("{brandId}/products")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductsListResponse>> GetBrandProducts(
        [FromRoute] long brandId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string? category = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] ProductSortType sortBy = ProductSortType.Newest)
    {
        var query = new GetProductsQuery(brandId, page, pageSize, minPrice, maxPrice, category, searchTerm, sortBy);
        var response = await _productService.GetProductsByBrandAsync(query);
        
        return Ok(response);
    }
    
    [HttpPut("{brandId:long}")]
    [Authorize(Roles = "Administrator,Moderator,SuperVendor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBrand(
        [FromRoute] long brandId,
        [FromBody] UpdateBrandRequest request,
        [FromServices] UserHelper userHelper)
    {
        var currentUserId = userHelper.GetUserId();
        var isAdminOrModerator = userHelper.IsAdminOrModerator();

        try
        {
            await _brandService.UpdateBrandAsync(brandId, request, currentUserId, isAdminOrModerator);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title  = "Brand not found",
                Detail = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title  = "Forbidden",
                Detail = ex.Message
            });
        }
    }

}
