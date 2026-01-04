using IndeConnect_Back.Application.DTOs.Categories;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/categories")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all available categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryLookupDto>>> GetCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }
}