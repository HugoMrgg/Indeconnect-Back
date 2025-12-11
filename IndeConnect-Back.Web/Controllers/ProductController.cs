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
    [HttpGet("{productId:long}/reviews")]
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
    
    
    [Authorize] // vendeur
    [HttpGet("seller/{productId:long}/reviews")]
    public async Task<IActionResult> GetAllProductReviewsForSeller(
        long productId,
        [FromServices] UserHelper userHelper)
    {
        var sellerId = userHelper.GetUserId();

        // on peut vérifier ici IsSellerOfProductAsync via le service
        var reviews = await _productService.GetAllProductReviewsAsync(productId);
        return Ok(reviews);
    }
    
    [Authorize] // vendeur
    [HttpPost("reviews/{reviewId:long}/approve")]
    public async Task<IActionResult> ApproveReview(
        long reviewId,
        [FromServices] UserHelper userHelper)
    {
        var sellerId = (long) userHelper.GetUserId();

        try
        {
            await _productService.ApproveProductReviewAsync(reviewId, sellerId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Review not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [Authorize] // vendeur
    [HttpPost("reviews/{reviewId:long}/reject")]
    public async Task<IActionResult> RejectReview(
        long reviewId,
        [FromServices] UserHelper userHelper)
    {
        var sellerId = (long) userHelper.GetUserId();

        try
        {
            await _productService.RejectProductReviewAsync(reviewId, sellerId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Review not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }



    
    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost("create")]
    //[Authorize(Policy = "CanManageProducts")] // à adapter quand vous aurez la policy
    [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateProductResponse>> CreateProduct([FromBody] CreateProductQuery query)
    {
        var response = await _productService.CreateProductAsync(query);

        return CreatedAtAction(
            nameof(GetProductById),
            new { productId = response.Id },
            response
        );
    }
    
    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{productId:long}")]
    //[Authorize(Policy = "CanManageProducts")] // ou à désactiver en dev
    [ProducesResponseType(typeof(UpdateProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateProductResponse>> UpdateProduct(
        [FromRoute] long productId,
        [FromBody] UpdateProductQuery query)
    {
        try
        {
            var response = await _productService.UpdateProductAsync(productId, query);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Product not found",
                Status = StatusCodes.Status404NotFound,
                Detail = $"No product with id {productId}."
            });
        }
    }
    
    [Authorize] // client authentifié
    [HttpPost("{productId:long}/reviews")]
    public async Task<IActionResult> AddReview(
        long productId,
        [FromBody] AddProductReviewRequest request,
        [FromServices] UserHelper userHelper)
    {
        var userId = (long) userHelper.GetUserId();
    
        try
        {
            var review = await _productService.AddProductReviewAsync(
                productId,
                userId,
                request.Rating,
                request.Comment
            );

            return CreatedAtAction(nameof(GetProductReviews), new { productId }, review);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Product not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


}
