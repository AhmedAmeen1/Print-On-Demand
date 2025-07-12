using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize(Roles = "User")]
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
                CustomProduct = new CustomProductResponseDTO
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

            // Check if product exists
            var customProduct = await _context.CustomProducts
                .FirstOrDefaultAsync(cp => cp.CustomProductId == dto.CustomProductId);
            if (customProduct == null) return BadRequest("Invalid product");

            // Check if item already in cart
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.UserId == userId &&
                    ci.CustomProductId == dto.CustomProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    CustomProductId = dto.CustomProductId,
                    Quantity = dto.Quantity,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Cart/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartItemId == id &&
                    ci.UserId == userId);

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
                .FirstOrDefaultAsync(ci =>
                    ci.CartItemId == id &&
                    ci.UserId == userId);

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

            if (!cartItems.Any()) return BadRequest("Cart is empty");

            // Create order
            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = shippingAddress
            };

            // Add order items
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

            // Clear cart
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(OrdersController.GetOrder),
                new { id = order.OrderId },
                MapToOrderResponse(order));
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
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDTO
                {
                    OrderItemId = oi.OrderItemId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    CustomProduct = new CustomProductResponseDTO
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
                }).ToList(),
                Payments = order.Payments.Select(p => new PaymentResponseDTO
                {
                    PaymentId = p.PaymentId,
                    Amount = p.Amount,
                    Status = p.Status,
                    Method = p.Method,
                    PaymentDate = p.PaymentDate,
                    TransactionId = p.TransactionId
                }).ToList()
            };
        }
    }
}
