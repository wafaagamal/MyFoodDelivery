using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Payments.Dtos;

public class ConfirmPaymentDto
{
    [Required]
    public string PaymentIntentId { get; set; } = default!;

    [Required]
    public string PaymentMethodId { get; set; } = default!;
}

