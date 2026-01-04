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
    private readonly UserHelper _userHelper;
    private readonly IBrandRequestMailService _mailService;

    public BrandController(IBrandService brandService, IProductService productService, UserHelper userHelper, IBrandRequestMailService mailService) 
    {
        _brandService = brandService;
        _productService = productService;
        _userHelper = userHelper;
        _mailService = mailService;
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
        var userId =  _userHelper.GetUserId();
        var response = await _productService.GetProductsByBrandAsync(query, userId);
        
        return Ok(response);
    }
    
    [HttpPut("{brandId:long}")]
    [Authorize(Roles = "SuperVendor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBrand(
        [FromRoute] long brandId,
        [FromBody] UpdateBrandRequest request,
        [FromServices] UserHelper userHelper)
    {
        var currentUserId = userHelper.GetUserId();

        try
        {
            await _brandService.UpdateBrandAsync(brandId, request, currentUserId);
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
    /**
    * Get my brand (for SuperVendor editing/preview)
    */
    [HttpGet("my-brand")]
    [Authorize(Roles = "SuperVendor, Vendor")]
    [ProducesResponseType(typeof(BrandDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandDetailDto>> GetMyBrand(
        [FromServices] UserHelper userHelper)
    {
        var currentUserId = userHelper.GetUserId();
    
        var brand = await _brandService.GetMyBrandAsync(currentUserId);
    
        if (brand == null)
            return NotFound(new { message = "No brand associated with your account" });

        return Ok(brand);
    }
    [HttpPut("my-brand/deposit")]
    [Authorize(Roles = "SuperVendor")]
    [ProducesResponseType(typeof(DepositDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepositDto>> UpsertMyBrandDeposit(
        [FromBody] UpsertBrandDepositRequest request,
        [FromServices] UserHelper userHelper)
    {
        var currentUserId = userHelper.GetUserId();

        try
        {
            var deposit = await _brandService.UpsertMyBrandDepositAsync(currentUserId, request);
            return Ok(deposit);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Brand not found",
                Detail = ex.Message
            });
        }
    }
    
    [HttpPost("request")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] BecomeBrandRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.BrandName))
            return BadRequest("BrandName est requis.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email est requis.");

        await _mailService.SendBecomeBrandRequestAsync(request);
        return Ok(new { message = "Demande envoyée." });
    }
    // ============================================================================
    // MODERATION ROUTES
    // ============================================================================

    /// <summary>
    /// SuperVendor soumet sa marque pour validation
    /// </summary>
    [HttpPost("{brandId}/submit")]
    [Authorize(Roles = "SuperVendor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitBrand(
        [FromRoute] long brandId,
        [FromServices] UserHelper userHelper)
    {
        var currentUserId = userHelper.GetUserId();

        if (!currentUserId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        try
        {
            await _brandService.SubmitBrandAsync(brandId, currentUserId.Value);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Brand not found",
                Detail = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid operation",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Moderator récupère la liste des marques à modérer
    /// </summary>
    [HttpGet("moderation")]
    [Authorize(Roles = "Moderator,Administrator")]
    [ProducesResponseType(typeof(IEnumerable<BrandModerationListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BrandModerationListDto>>> GetBrandsForModeration()
    {
        var brands = await _brandService.GetBrandsForModerationAsync();
        return Ok(brands);
    }

    /// <summary>
    /// Moderator récupère les détails d'une marque pour modération
    /// </summary>
    [HttpGet("moderation/{brandId}")]
    [Authorize(Roles = "Moderator,Administrator")]
    [ProducesResponseType(typeof(BrandModerationDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandModerationDetailDto>> GetBrandForModeration(
        [FromRoute] long brandId)
    {
        var brand = await _brandService.GetBrandForModerationAsync(brandId);

        if (brand == null)
            return NotFound(new { message = "Brand not found" });

        return Ok(brand);
    }

    /// <summary>
    /// Moderator approuve une marque
    /// </summary>
    [HttpPost("{brandId}/approve")]
    [Authorize(Roles = "Moderator,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveBrand(
        [FromRoute] long brandId,
        [FromServices] UserHelper userHelper)
    {
        var moderatorUserId = userHelper.GetUserId();

        if (!moderatorUserId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        try
        {
            await _brandService.ApproveBrandAsync(brandId, moderatorUserId.Value);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Brand not found",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid operation",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Moderator rejette une marque avec un commentaire
    /// </summary>
    [HttpPost("{brandId}/reject")]
    [Authorize(Roles = "Moderator,Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectBrand(
        [FromRoute] long brandId,
        [FromBody] RejectBrandRequest request,
        [FromServices] UserHelper userHelper)
    {
        var moderatorUserId = userHelper.GetUserId();

        if (!moderatorUserId.HasValue)
            return Unauthorized(new { message = "User not authenticated" });

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest(new ProblemDetails
            {
                Title = "Validation error",
                Detail = "Rejection reason is required"
            });

        try
        {
            await _brandService.RejectBrandAsync(brandId, moderatorUserId.Value, request.Reason);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Brand not found",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid operation",
                Detail = ex.Message
            });
        }
    }

}
