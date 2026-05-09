using System;

namespace OrderingSvc.Domain.Services;

public class PaymentIntentResult
{
    public bool Success { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool RequiresAction { get; set; }
    public string? ActionUrl { get; set; }
}

public class PaymentConfirmation
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RefundResult
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public decimal Amount { get; set; }
    public string? ErrorMessage { get; set; }
}
