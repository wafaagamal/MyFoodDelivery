namespace OrderingSvc.Application.Contracts.Payments.Dtos;

public class PaymentIntentDto
{
    public string PaymentIntentId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!;
}

