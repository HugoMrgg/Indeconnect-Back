using IndeConnect_Back.Application.DTOs.Ethics;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/ethics/catalog")]
public class CatalogueController : ControllerBase
{
    private readonly ICatalogService _service;

    public CatalogueController(ICatalogService service) =>_service = service;
    
    [HttpGet]
    [Authorize(Roles = "Administrator")] // adapte selon tes rôles
    public async Task<ActionResult<AdminCatalogDto>> GetCatalog()
        =>Ok(await _service.GetCatalogAsync());

    [HttpPut]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<AdminCatalogDto>> UpsertCatalog([FromBody] AdminUpsertCatalogRequest request)
        => Ok(await _service.UpsertCatalogAsync(request));
}