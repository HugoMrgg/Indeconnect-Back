using IndeConnect_Back.Application.DTOs.Ethics;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/admin/ethics")]
[Authorize(Roles = "Administrator")]
public class AdminEthicsController : ControllerBase
{
    private readonly IEthicsAdminService _service;
    private readonly UserHelper _userHelper;

    public AdminEthicsController(IEthicsAdminService service, UserHelper userHelper)
    {
        _service = service;
        _userHelper = userHelper;
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<AdminUpsertCatalogRequest>> GetCatalog()
        => Ok(await _service.GetCatalogAsync());

    [HttpPut("catalog")]
    public async Task<IActionResult> UpsertCatalog([FromBody] AdminUpsertCatalogRequest request)
    {
        await _service.UpsertCatalogAsync(request);
        return NoContent();
    }

    [HttpPut("questionnaires/{questionnaireId:long}/review")]
    public async Task<IActionResult> Review(long questionnaireId, [FromBody] ReviewQuestionnaireRequest request)
    {
        var adminId = _userHelper.GetUserId();
        if (!adminId.HasValue) return Unauthorized();
        await _service.ReviewQuestionnaireAsync(questionnaireId, adminId.Value, request);
        return NoContent();
    }
}