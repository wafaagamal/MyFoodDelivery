using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Services;
using System.Threading.Tasks;

namespace OrderingSvc.Application.Contracts.Payments;

#region Payment DTOs

public class PaymentIntentDto
{
    public string PaymentIntentId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!;
}

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class RefundResultDto
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public decimal Amount { get; set; }
    public string? ErrorMessage { get; set; }
}

#endregion

#region Payment Commands

public class CreatePaymentIntentDto
{
    [Required]
    public Guid OrderId { get; set; }
}

public class ConfirmPaymentDto
{
    [Required]
    public string PaymentIntentId { get; set; } = default!;

    [Required]
    public string PaymentMethodId { get; set; } = default!;
}

public class ProcessRefundDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Range(0.01, 100000)]
    public decimal? Amount { get; set; }  // Null = full refund

    [StringLength(500)]
    public string? Reason { get; set; }
}

#endregion

/// <summary>
/// ABP Application Service interface for payment processing.
/// </summary>
public interface IPaymentAppService : IApplicationService
{
    /// <summary>
    /// Create a payment intent for Stripe
    /// </summary>
    Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentIntentDto input);

    /// <summary>
    /// Confirm a payment
    /// </summary>
    Task<PaymentResultDto> ConfirmPaymentAsync(ConfirmPaymentDto input);

    /// <summary>
    /// Process a refund
    /// </summary>
    Task<RefundResultDto> ProcessRefundAsync(ProcessRefundDto input);

    /// <summary>
    /// Handle Stripe webhook events
    /// </summary>
    Task HandleWebhookAsync(string payload, string signature);
}
