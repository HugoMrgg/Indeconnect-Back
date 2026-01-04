using IndeConnect_Back.Application.DTOs.Products;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/product-groups")]
public class ProductGroupController : ControllerBase
{
    private readonly IProductGroupService _productGroupService;

    public ProductGroupController(IProductGroupService productGroupService)
    {
        _productGroupService = productGroupService;
    }

    /// <summary>
    /// Crée un nouveau ProductGroup pour la marque du SuperVendor/Vendor
    /// Utilisé quand le SuperVendor/Vendor veut créer un nouveau type de produit
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperVendor,Vendor")] 
    [ProducesResponseType(typeof(ProductGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductGroupDto>> CreateProductGroup(
        [FromBody] CreateProductGroupRequest request,
        [FromServices] UserHelper userHelper)
    {
        var currentUserId = userHelper.GetUserId();

        try
        {
            var productGroup = await _productGroupService.CreateProductGroupAsync(request, currentUserId);

            return CreatedAtAction(
                nameof(GetProductGroupById),
                new { productGroupId = productGroup.Id },
                productGroup
            );
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid input",
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
    /// Récupère un ProductGroup par son ID avec tous ses produits (variantes de couleur)
    /// </summary>
    [HttpGet("{productGroupId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductGroupDto>> GetProductGroupById([FromRoute] long productGroupId)
    {
        var productGroup = await _productGroupService.GetProductGroupByIdAsync(productGroupId);

        if (productGroup == null)
            return NotFound(new { message = "Product group not found" });

        return Ok(productGroup);
    }

    /// <summary>
    /// Liste tous les ProductGroups d'une marque
    /// Utilisé pour le dropdown de sélection lors de la création d'un produit
    /// </summary>
    [HttpGet("brand/{brandId}")]
    [Authorize(Roles = "SuperVendor,Vendor")] // ✅ Ajouté Vendor
    [ProducesResponseType(typeof(IEnumerable<ProductGroupSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductGroupSummaryDto>>> GetProductGroupsByBrand([FromRoute] long brandId)
    {
        var productGroups = await _productGroupService.GetProductGroupsByBrandAsync(brandId);
        return Ok(productGroups);
    }

    /// <summary>
    /// Met à jour un ProductGroup (nom, description, catégorie)
    /// </summary>
    [HttpPut("{productGroupId}")]
    [Authorize(Roles = "SuperVendor,Vendor")] // ✅ Ajouté Vendor
    [ProducesResponseType(typeof(ProductGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductGroupDto>> UpdateProductGroup(
        [FromRoute] long productGroupId,
        [FromBody] UpdateProductGroupRequest request,
        [FromServices] UserHelper userHelper)
    {
        var currentUserId = userHelper.GetUserId();

        try
        {
            var productGroup = await _productGroupService.UpdateProductGroupAsync(productGroupId, request, currentUserId);
            return Ok(productGroup);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Product group not found",
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
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid input",
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