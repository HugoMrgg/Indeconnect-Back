using IndeConnect_Back.Application.DTOs.Ethics;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/ethics/questionnaire")]
public class EthicsQuestionnaireController : ControllerBase
{
    private readonly IEthicsQuestionnaireService _service;
    private readonly UserHelper _userHelper;

    public EthicsQuestionnaireController(IEthicsQuestionnaireService service, UserHelper userHelper)
    {
        _service = service;
        _userHelper = userHelper;
    }

    [HttpGet]
    [Authorize(Roles = "SuperVendor")]
    public async Task<ActionResult<EthicsFormDto>> GetMyForm()
    {
        var userId = _userHelper.GetUserId();
        if (!userId.HasValue) return Unauthorized();
        return Ok(await _service.GetMyEthicsFormAsync(userId.Value));
    }

    [HttpPut]
    [Authorize(Roles = "SuperVendor")]
    public async Task<ActionResult<EthicsFormDto>> Upsert([FromBody] UpsertQuestionnaireRequest request)
    {
        var userId = _userHelper.GetUserId();
        if (!userId.HasValue) return Unauthorized();
        return Ok(await _service.UpsertMyQuestionnaireAsync(userId.Value, request));
    }
}