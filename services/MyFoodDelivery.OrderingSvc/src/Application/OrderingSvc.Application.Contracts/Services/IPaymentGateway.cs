using System;
using System.Threading.Tasks;

namespace OrderingSvc.Application.Services;

/// <summary>
/// Payment intent creation result
/// </summary>
public class PaymentIntentResult
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Payment processing result
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionUrl { get; set; }
}

/// <summary>
/// Payment confirmation result
/// </summary>
public class PaymentConfirmation
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Refund processing result
/// </summary>
public class RefundResult
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public decimal Amount { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Stripe settings configuration
/// </summary>
public class StripeSettings
{
    public string SecretKey { get; set; } = default!;
    public string PublishableKey { get; set; } = default!;
    public string WebhookSecret { get; set; } = default!;
}

/// <summary>
/// Internal payment gateway interface for Stripe operations
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId,
        decimal amount,
        string currency,
        string? customerEmail);

    Task<PaymentResult> ProcessPaymentAsync(
        string paymentIntentId,
        string paymentMethodId);

    Task<PaymentResult> ProcessCardPaymentAsync(
        Guid orderId,
        decimal amount,
        string cardToken);

    Task<PaymentConfirmation> ConfirmPaymentAsync(string paymentIntentId);

    Task<RefundResult> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        string reason);
}
