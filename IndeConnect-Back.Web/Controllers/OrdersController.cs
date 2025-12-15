using System.Security.Claims;
using IndeConnect_Back.Application.DTOs.Orders;
using IndeConnect_Back.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IndeConnect_Back.Web.Controllers
{
    [ApiController]
    [Route("indeconnect/orders")]
    [Authorize] 
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Crée une nouvelle commande à partir du panier de l'utilisateur
        /// POST /api/orders
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            // Récupérer l'ID de l'utilisateur connecté depuis le JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Utilisateur non authentifié" });
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(userId, request);
                
                // Retourne 201 Created avec l'URL de la ressource créée
                return CreatedAtAction(
                    nameof(GetOrder), 
                    new { orderId = order.Id }, 
                    order
                );
            }
            catch (InvalidOperationException ex)
            {
                // Erreurs métier (panier vide, stock insuffisant, etc.)
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Erreur serveur inattendue
                return StatusCode(500, new { message = "Erreur lors de la création de la commande", detail = ex.Message });
            }
        }

        /// <summary>
        /// Récupère une commande par son ID
        /// GET /api/orders/{orderId}
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDto>> GetOrder(long orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null)
            {
                return NotFound(new { message = "Commande introuvable" });
            }

            if (order.UserId != userId)
            {
                return Forbid(); // 403
            }

            return Ok(order);
        }

        /// <summary>
        /// Récupère toutes les commandes d'un utilisateur
        /// GET /api/users/{userId}/orders
        /// </summary>
        [HttpGet("users/{userId}")]
        public async Task<ActionResult<List<OrderDto>>> GetUserOrders(long userId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized();
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (currentUserId != userId && userRole != "Admin")
            {
                return Forbid(); // 403 Forbidden
            }

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }
    }
}
