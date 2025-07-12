using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using POD.DTO;
using POD.Models;
using Stripe;
using System.Security.Claims;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "User")]
public class PaymentsController : ControllerBase
{
    private readonly Context _context;
    private readonly StripeSettings _stripeSettings;

    public PaymentsController(Context context, IOptions<StripeSettings> stripeOptions)
    {
        _context = context;
        _stripeSettings = stripeOptions.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
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

    // POST: api/Payments/StripeWebhook
    [AllowAnonymous]
    [HttpPost("StripeWebhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var stripeEvent = EventUtility.ConstructEvent(
            json,
            Request.Headers["Stripe-Signature"],
            "your_webhook_secret_here" // Replace with your actual webhook secret
        );

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
                    Method = POD.Models.PaymentMethod.Card, // Fully qualified
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
