using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/users/{userId}/payment-methods")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _service;

    public PaymentMethodsController(IPaymentMethodService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaymentMethods(long userId)
    {
        var methods = await _service.GetUserPaymentMethodsAsync(userId);
        return Ok(methods);
    }

    [HttpDelete("{paymentMethodId}")]
    public async Task<IActionResult> DeletePaymentMethod(long userId, string paymentMethodId)
    {
        await _service.DeletePaymentMethodAsync(userId, paymentMethodId);
        return NoContent();
    }

    [HttpPut("{paymentMethodId}/default")]
    public async Task<IActionResult> SetDefaultPaymentMethod(long userId, string paymentMethodId)
    {
        var method = await _service.SetDefaultPaymentMethodAsync(userId, paymentMethodId);
        return Ok(method);
    }
}