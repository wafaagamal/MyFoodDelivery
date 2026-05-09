using System;
using System.Threading.Tasks;

namespace OrderingSvc.Domain.Services;

/// <summary>
/// Domain service interface for payment gateway operations.
/// Implemented in Infrastructure layer (Stripe).
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
