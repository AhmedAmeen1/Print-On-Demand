using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using POD.DTO;
using POD.Models;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "User")]
public class PaymentsController : ControllerBase
{
    private readonly Context _context;
    private readonly StripeSettings _stripeSettings;
    private readonly string _webhookSecret;

    public PaymentsController(Context context, IOptions<StripeSettings> stripeOptions)
    {
        _context = context;
        _stripeSettings = stripeOptions.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        _webhookSecret = _stripeSettings.WebhookSecret;
    }

    // POST: api/Payments/CreatePaymentIntent
    [HttpPost("CreatePaymentIntent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDTO dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.OrderId == dto.OrderId && o.UserId == userId);

        if (order == null) return NotFound("Order not found");

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(order.TotalAmount * 100), // Stripe works in cents
            Currency = "usd",
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.OrderId.ToString() },
                { "user_id", userId }
            }
        };

        var service = new PaymentIntentService();
        var paymentIntent = await service.CreateAsync(options);

        return Ok(new
        {
            clientSecret = paymentIntent.ClientSecret,
            publishableKey = _stripeSettings.PublishableKey
        });
    }

    // POST: api/Payments/CreateCheckoutSession
    [HttpPost("CreateCheckoutSession")]
    [AllowAnonymous] // Or [Authorize(Roles = "User")] if you want only logged-in users
    public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionDTO dto)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == dto.OrderId);
        if (order == null) return NotFound("Order not found");

        var domain = "https://print-on-demand.runasp.net";
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(order.TotalAmount * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order #{order.OrderId}"
                        }
                    },
                    Quantity = 1
                }
            },
            Mode = "payment",
            SuccessUrl = $"{domain}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{domain}/payment-cancel"
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return Ok(new { url = session.Url });
    }

    // POST: api/Payments/StripeWebhook
    [AllowAnonymous]
    [HttpPost("StripeWebhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _webhookSecret
            );
        }
        catch (StripeException e)
        {
            // Invalid signature or payload
            return BadRequest($"Stripe webhook error: {e.Message}");
        }
        catch (Exception ex)
        {
            // Other errors
            return BadRequest($"Webhook error: {ex.Message}");
        }

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;
            var orderId = int.Parse(paymentIntent.Metadata["order_id"]);
            var order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order != null)
            {
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = (decimal)paymentIntent.AmountReceived / 100,
                    Method = POD.Models.PaymentMethod.Card,
                    Status = PaymentStatus.Completed,
                    PaymentDate = DateTime.UtcNow,
                    TransactionId = paymentIntent.Id
                };
                _context.Payments.Add(payment);
                order.Status = OrderStatus.Processing;
                await _context.SaveChangesAsync();
            }
        }

        return Ok();
    }
}

// DTO for Checkout Session
public class CreateCheckoutSessionDTO
{
    public int OrderId { get; set; }
}
