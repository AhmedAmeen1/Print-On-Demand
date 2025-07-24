using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POD.DTO;
using POD.Models;
using System.Security.Claims;

namespace POD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(Context context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Cart
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartItemResponseDTO>>> GetCartItems()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.CartItems
                .Include(ci => ci.CustomProduct)
                .ThenInclude(cp => cp.ProductTemplate)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            return cartItems.Select(ci => new CartItemResponseDTO
            {
                CartItemId = ci.CartItemId,
                CustomProductId = ci.CustomProductId,
                Quantity = ci.Quantity,
                AddedAt = ci.AddedAt,
                CustomProduct = ci.CustomProduct == null ? null : new CustomProductResponseDTO
                {
                    CustomProductId = ci.CustomProduct.CustomProductId,
                    CustomName = ci.CustomProduct.CustomName,
                    CustomDescription = ci.CustomProduct.CustomDescription,
                    CustomImageUrl = ci.CustomProduct.CustomImageUrl,
                    Price = ci.CustomProduct.Price,
                    CreatedAt = ci.CustomProduct.CreatedAt,
                    ProductTemplateId = ci.CustomProduct.ProductTemplateId,
                    UserId = ci.CustomProduct.UserId
                }
            }).ToList();
        }

        // POST: api/Cart
        [HttpPost]
        public async Task<ActionResult<CartItemResponseDTO>> AddToCart(CartItemDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var customProduct = await _context.CustomProducts
                .FirstOrDefaultAsync(cp => cp.CustomProductId == dto.CustomProductId);
            if (customProduct == null)
                return BadRequest("Invalid product");

            var existingItem = await _context.CartItems
                .Include(ci => ci.CustomProduct)
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.CustomProductId == dto.CustomProductId);

            if (existingItem != null)
                existingItem.Quantity += dto.Quantity;
            else
            {
                existingItem = new CartItem
                {
                    UserId = userId,
                    CustomProductId = dto.CustomProductId,
                    Quantity = dto.Quantity,
                    AddedAt = DateTime.UtcNow,
                    CustomProduct = customProduct
                };
                _context.CartItems.Add(existingItem);
            }

            await _context.SaveChangesAsync();

            var cartItemResponse = new CartItemResponseDTO
            {
                CartItemId = existingItem.CartItemId,
                CustomProductId = existingItem.CustomProductId,
                Quantity = existingItem.Quantity,
                AddedAt = existingItem.AddedAt,
                CustomProduct = existingItem.CustomProduct == null ? null : new CustomProductResponseDTO
                {
                    CustomProductId = existingItem.CustomProduct.CustomProductId,
                    CustomName = existingItem.CustomProduct.CustomName,
                    CustomDescription = existingItem.CustomProduct.CustomDescription,
                    CustomImageUrl = existingItem.CustomProduct.CustomImageUrl,
                    Price = existingItem.CustomProduct.Price,
                    CreatedAt = existingItem.CustomProduct.CreatedAt,
                    ProductTemplateId = existingItem.CustomProduct.ProductTemplateId,
                    UserId = existingItem.CustomProduct.UserId
                }
            };

            return Ok(cartItemResponse);
        }

        // PUT: api/Cart/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == id && ci.UserId == userId);

            if (cartItem == null) return NotFound();
            if (quantity <= 0) return BadRequest("Quantity must be positive");

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Cart/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == id && ci.UserId == userId);

            if (cartItem == null) return NotFound();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Cart/Checkout
        [HttpPost("Checkout")]
        public async Task<ActionResult<OrderResponseDTO>> Checkout([FromBody] string shippingAddress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _context.CartItems
                .Include(ci => ci.CustomProduct)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
                return BadRequest("Cart is empty");

            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = shippingAddress
            };

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    CustomProductId = cartItem.CustomProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.CustomProduct.Price,
                    TotalPrice = cartItem.Quantity * cartItem.CustomProduct.Price
                };
                order.OrderItems.Add(orderItem);
                order.TotalAmount += orderItem.TotalPrice;
            }

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            var createdOrder = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.CustomProduct)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            if (createdOrder == null)
                return NotFound("Order created but not found");

            try
            {
                var response = MapToOrderResponse(createdOrder);
                // <-- FIXED: specify controller name "Orders"
                return CreatedAtAction(
                    actionName: "GetOrder",
                    controllerName: "Orders",
                    routeValues: new { id = createdOrder.OrderId },
                    value: response);
            }
            catch (Exception ex)
            {
                return Problem(detail: $"Error mapping order response: {ex.Message}", statusCode: 500);
            }
        }


        private OrderResponseDTO MapToOrderResponse(Order order)
        {
            return new OrderResponseDTO
            {
                OrderId = order.OrderId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                OrderDate = order.OrderDate,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                ShippingAddress = order.ShippingAddress,
                OrderItems = order.OrderItems?.Select(oi => new OrderItemResponseDTO
                {
                    OrderItemId = oi.OrderItemId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    CustomProduct = oi.CustomProduct == null ? null : new CustomProductResponseDTO
                    {
                        CustomProductId = oi.CustomProduct.CustomProductId,
                        CustomName = oi.CustomProduct.CustomName,
                        CustomDescription = oi.CustomProduct.CustomDescription,
                        CustomImageUrl = oi.CustomProduct.CustomImageUrl,
                        Price = oi.CustomProduct.Price,
                        CreatedAt = oi.CustomProduct.CreatedAt,
                        ProductTemplateId = oi.CustomProduct.ProductTemplateId,
                        UserId = oi.CustomProduct.UserId
                    }
                }).ToList() ?? new List<OrderItemResponseDTO>(),
                Payments = order.Payments?.Select(p => new PaymentResponseDTO
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    Status = p.Status,
                    Method = p.Method,
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId
                }).ToList() ?? new List<PaymentResponseDTO>()
            };
        }
    }
}
