using IndeConnect_Back.Application.DTOs.Payments;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/me/payment-methods")]
[Authorize]
public class MyPaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly UserHelper _userHelper;

    public MyPaymentMethodsController(IPaymentMethodService paymentMethodService, UserHelper userHelper)
    {
        _paymentMethodService = paymentMethodService;
        _userHelper = userHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userId = _userHelper.GetUserId();
        var res = await _paymentMethodService.GetUserPaymentMethodsAsync(userId);
        return Ok(res);
    }

    [HttpDelete("{paymentMethodId}")]
    public async Task<IActionResult> Delete(string paymentMethodId)
    {
        var userId = _userHelper.GetUserId();
        await _paymentMethodService.DeletePaymentMethodAsync(userId, paymentMethodId);
        return NoContent();
    }

    [HttpPost("{paymentMethodId}/default")]
    public async Task<IActionResult> SetDefault(string paymentMethodId)
    {
        var userId = _userHelper.GetUserId();
        var dto = await _paymentMethodService.SetDefaultPaymentMethodAsync(userId, paymentMethodId);
        return Ok(dto);
    }
    
    [HttpPost("setup-intent")]
    public async Task<ActionResult<SetupIntentResponse>> CreateSetupIntent()
    {
        var userId = _userHelper.GetUserId();
        var clientSecret = await _paymentMethodService.CreateSetupIntentAsync(userId);
        return Ok(new SetupIntentResponse { ClientSecret = clientSecret });
    }
}