using System;

namespace OrderingSvc.Application.Contracts.Payments.Dtos;

public class PaymentResultDto
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

