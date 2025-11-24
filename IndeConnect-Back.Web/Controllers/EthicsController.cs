using IndeConnect_Back.Application.DTOs.Brands;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/ethics")]
public class EthicsController : ControllerBase
{
    private readonly IEthicsService _ethicsService;

    public EthicsController(IEthicsService ethicsService)
    {
        _ethicsService = ethicsService;
    }

    /// <summary>
    /// Récupère tous les tags éthiques disponibles
    /// Utilisé pour alimenter les filtres dynamiques du frontend
    /// </summary>
    /// <returns>Liste des tags avec leur label et nombre de marques</returns>
    [HttpGet("tags")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(EthicTagsListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<EthicTagsListResponse>> GetEthicTags()
    {
        var response = await _ethicsService.GetAllEthicTagsAsync();
        return Ok(response);
    }
}