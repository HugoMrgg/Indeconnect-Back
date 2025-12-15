using IndeConnect_Back.Application.DTOs.Payments;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers;

[ApiController]
[Route("indeconnect/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;
    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Crée un PaymentIntent Stripe pour une commande
    /// POST /api/payments/create-intent
    /// </summary>
    [HttpPost("create-intent")]
    public async Task<ActionResult<PaymentIntentResponse>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request)
    {
        try
        {
            var clientSecret = await _paymentService.CreatePaymentIntentAsync(request.OrderId);

            return Ok(new PaymentIntentResponse
            {
                ClientSecret = clientSecret,
                OrderId = request.OrderId,
                // Récupérer amount et currency depuis l'order si besoin
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Confirme le paiement et met à jour la commande
    /// POST /indeconnect/payments/confirm
    /// </summary>
    [HttpPost("confirm")]
    public async Task<ActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            var payment = await _paymentService.ConfirmPaymentAsync(
                request.OrderId,
                request.PaymentIntentId
            );

            return Ok(new
            {
                success = true,
                orderId = payment.OrderId,
                paymentStatus = payment.Status.ToString()
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment for order {OrderId}", request.OrderId);
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}