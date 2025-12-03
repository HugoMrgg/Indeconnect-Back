using IndeConnect_Back.Application.DTOs.Products;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get detailed product information (includes color variants from ProductGroup)
    /// </summary>
    [HttpGet("{productId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetProductById([FromRoute] long productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }

    /// <summary>
    /// Get all size variants of a product with their stock
    /// </summary>
    [HttpGet("{productId}/variants")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductVariantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ProductVariantDto>>> GetProductVariants([FromRoute] long productId)
    {
        var variants = await _productService.GetProductVariantsAsync(productId);

        if (!variants.Any())
            return NotFound(new { message = "Product not found or has no size variants" });

        return Ok(variants);
    }

    /// <summary>
    /// NOUVEAU : Get color variants (other products in the same ProductGroup)
    /// </summary>
    [HttpGet("{productId}/colors")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductColorVariantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductColorVariantDto>>> GetProductColorVariants([FromRoute] long productId)
    {
        var colorVariants = await _productService.GetProductColorVariantsAsync(productId);
        return Ok(colorVariants);
    }

    /// <summary>
    /// Get total stock information for a product
    /// </summary>
    [HttpGet("{productId}/stock")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductStockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductStockDto>> GetProductStock([FromRoute] long productId)
    {
        var stock = await _productService.GetProductStockAsync(productId);

        if (stock == null)
            return NotFound(new { message = "Product not found" });

        return Ok(stock);
    }

    /// <summary>
    /// Get approved reviews for a product
    /// </summary>
    [HttpGet("{productId}/reviews")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductReviewDto>>> GetProductReviews(
        [FromRoute] long productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var reviews = await _productService.GetProductReviewsAsync(productId, page, pageSize);
        return Ok(reviews);
    }
    
    /// <summary>
    /// Create a product
    /// </summary>
    [HttpPost("create")]
    [Authorize(Policy = "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductReviewDto>>> CreateProduct([FromBody] CreateProductQuery query)
    {
        var create = await _productService.CreateProductAsync(query);
        return Ok(create);
    }
    
    /// <summary>
    /// Create a product
    /// </summary>
    [HttpPost("update")]
    [Authorize(Policy = "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductReviewDto>>> UpdateProduct([FromBody] UpdateProductQuery query)
    {
        var create = await _productService.UpdateProductAsync(query);
        return Ok(create);
    }
}
