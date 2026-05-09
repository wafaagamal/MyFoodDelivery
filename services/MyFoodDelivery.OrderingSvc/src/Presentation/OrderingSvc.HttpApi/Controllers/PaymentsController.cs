using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderingSvc.Application.Contracts.Payments;
using OrderingSvc.Application.Contracts.Payments.Dtos;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;

namespace OrderingSvc.HttpApi.Controllers;

/// <summary>
/// REST API controller for payment processing.
/// Uses ABP Application Services.
/// </summary>
[RemoteService]
[Area("ordering")]
[Route("api/payments")]
public class PaymentsController : AbpControllerBase
{
    private readonly IPaymentAppService _paymentAppService;

    public PaymentsController(IPaymentAppService paymentAppService)
    {
        _paymentAppService = paymentAppService;
    }

    /// <summary>
    /// Create a payment intent (Stripe)
    /// </summary>
    [HttpPost("create-intent")]
    [Authorize]
    public async Task<ActionResult<PaymentIntentDto>> CreatePaymentIntentAsync(
        [FromBody] CreatePaymentIntentDto input)
    {
        var result = await _paymentAppService.CreatePaymentIntentAsync(input);
        return Ok(result);
    }

    /// <summary>
    /// Confirm a payment
    /// </summary>
    [HttpPost("confirm")]
    [Authorize]
    public async Task<ActionResult<PaymentResultDto>> ConfirmPaymentAsync(
        [FromBody] ConfirmPaymentDto input)
    {
        var result = await _paymentAppService.ConfirmPaymentAsync(input);
        return Ok(result);
    }

    /// <summary>
    /// Process a refund
    /// </summary>
    [HttpPost("refund")]
    [Authorize]
    public async Task<ActionResult<RefundResultDto>> ProcessRefundAsync(
        [FromBody] ProcessRefundDto input)
    {
        var result = await _paymentAppService.ProcessRefundAsync(input);
        return Ok(result);
    }

    /// <summary>
    /// Stripe webhook endpoint
    /// </summary>
    [HttpPost("webhook/stripe")]
    [AllowAnonymous]
    public async Task<ActionResult> StripeWebhookAsync()
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = HttpContext.Request.Headers["Stripe-Signature"].ToString();

        await _paymentAppService.HandleWebhookAsync(payload, signature);
        return Ok();
    }
}
