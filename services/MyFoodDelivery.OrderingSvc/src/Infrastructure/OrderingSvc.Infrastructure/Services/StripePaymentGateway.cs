using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrderingSvc.Domain.Services;
using Stripe;

namespace OrderingSvc.Infrastructure.Services;

/// <summary>
/// Stripe payment gateway implementation
/// </summary>
public class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeSettings _settings;

    public StripePaymentGateway(IOptions<StripeSettings> settings)
    {
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId,
        decimal amount,
        string currency,
        string? customerEmail)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = currency.ToLower(),
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", orderId.ToString() }
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            if (!string.IsNullOrEmpty(customerEmail))
            {
                options.ReceiptEmail = customerEmail;
            }

            var service = new PaymentIntentService();
            var intent = await service.CreateAsync(options);

            return new PaymentIntentResult
            {
                Success = true,
                PaymentIntentId = intent.Id,
                ClientSecret = intent.ClientSecret
            };
        }
        catch (StripeException ex)
        {
            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentResult> ProcessPaymentAsync(
        string paymentIntentId,
        string paymentMethodId)
    {
        try
        {
            var service = new PaymentIntentService();
            var intent = await service.ConfirmAsync(paymentIntentId, new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId
            });

            if (intent.Status == "succeeded")
            {
                return new PaymentResult
                {
                    Success = true,
                    TransactionId = intent.Id
                };
            }

            if (intent.Status == "requires_action")
            {
                return new PaymentResult
                {
                    Success = false,
                    RequiresAction = true,
                    ActionUrl = intent.NextAction?.RedirectToUrl?.Url
                };
            }

            return new PaymentResult
            {
                Success = false,
                ErrorMessage = $"Payment status: {intent.Status}"
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentResult> ProcessCardPaymentAsync(
        Guid orderId,
        decimal amount,
        string cardToken)
    {
        try
        {
            var chargeOptions = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = "usd",
                Source = cardToken,
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", orderId.ToString() }
                }
            };

            var service = new ChargeService();
            var charge = await service.CreateAsync(chargeOptions);

            return new PaymentResult
            {
                Success = charge.Status == "succeeded",
                TransactionId = charge.Id,
                ErrorMessage = charge.Status != "succeeded" ? charge.FailureMessage : null
            };
        }
        catch (StripeException ex)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentConfirmation> ConfirmPaymentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var intent = await service.GetAsync(paymentIntentId);

            return new PaymentConfirmation
            {
                Success = intent.Status == "succeeded",
                Status = intent.Status,
                TransactionId = intent.Id
            };
        }
        catch (StripeException ex)
        {
            return new PaymentConfirmation
            {
                Success = false,
                Status = "error"
            };
        }
    }

    public async Task<RefundResult> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        string reason)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = transactionId,
                Amount = (long)(amount * 100),
                Reason = MapRefundReason(reason),
                Metadata = new Dictionary<string, string>
                {
                    { "reason", reason }
                }
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            return new RefundResult
            {
                Success = refund.Status == "succeeded",
                RefundId = refund.Id,
                ErrorMessage = refund.Status != "succeeded" ? refund.FailureReason : null
            };
        }
        catch (StripeException ex)
        {
            return new RefundResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static string MapRefundReason(string reason)
    {
        if (reason.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            return "duplicate";
        if (reason.Contains("fraud", StringComparison.OrdinalIgnoreCase))
            return "fraudulent";
        return "requested_by_customer";
    }
}
