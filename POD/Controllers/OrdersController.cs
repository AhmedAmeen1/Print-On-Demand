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
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(Context context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isSeller = User.IsInRole("Seller");

            IQueryable<Order> query = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.CustomProduct)
                        .ThenInclude(cp => cp.ProductTemplate)
                .Include(o => o.Payments);

            if (!isSeller)
            {
                query = query.Where(o => o.UserId == userId);
            }
            else
            {
                var sellerProducts = _context.ProductTemplates
                    .Where(pt => pt.SellerProfile.UserId == userId)
                    .Select(pt => pt.ProductTemplateId);

                query = query.Where(o => o.OrderItems
                    .Any(oi => sellerProducts.Contains(oi.CustomProduct.ProductTemplateId)));
            }

            var orders = await query.ToListAsync();
            return orders.Select(MapToOrderResponse).ToList();
        }

        // GET: api/Orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDTO>> GetOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.CustomProduct)
                        .ThenInclude(cp => cp.ProductTemplate)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            if (order.UserId != userId && !User.IsInRole("Seller"))
                return Forbid();

            return MapToOrderResponse(order);
        }

        // GET: api/Orders/{orderId}/Payments/{paymentId}
        [HttpGet("{orderId}/Payments/{paymentId}")]
        public async Task<ActionResult<PaymentResponseDTO>> GetPayment(int orderId, int paymentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            // Authorization: Only order owner or seller can access
            if (order.UserId != userId && !User.IsInRole("Seller"))
                return Forbid();

            var payment = order.Payments.FirstOrDefault(p => p.PaymentId == paymentId);
            if (payment == null)
                return NotFound();

            return MapToPaymentResponse(payment);
        }


        // POST: api/Orders/{id}/Payments
        [HttpPost("{id}/Payments")]
        public async Task<ActionResult<PaymentResponseDTO>> AddPayment(int id, PaymentDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            if (order.UserId != userId) return Forbid();

            var payment = new Payment
            {
                OrderId = id,
                Amount = dto.Amount,
                Method = dto.Method,
                Status = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow,
                TransactionId = dto.TransactionId
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Check if fully paid
            var totalPaid = order.Payments.Sum(p => p.Amount) + dto.Amount;
            if (totalPaid >= order.TotalAmount)
            {
                order.Status = OrderStatus.Processing;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetPayment),
                new { orderId = id, paymentId = payment.PaymentId },
                MapToPaymentResponse(payment));
        }

        // Helpers
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
                Payments = order.Payments.Select(MapToPaymentResponse).ToList()
            };
        }

        private PaymentResponseDTO MapToPaymentResponse(Payment payment)
        {
            return new PaymentResponseDTO
            {
                PaymentId = payment.PaymentId,
                Amount = payment.Amount,
                Status = payment.Status,
                Method = payment.Method,
                PaymentDate = payment.PaymentDate,
                TransactionId = payment.TransactionId
            };
        }
    }
}