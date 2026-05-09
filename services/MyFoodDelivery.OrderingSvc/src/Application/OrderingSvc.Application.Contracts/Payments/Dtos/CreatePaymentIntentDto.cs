using System;
using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Payments.Dtos;

public class CreatePaymentIntentDto
{
    [Required]
    public Guid OrderId { get; set; }
}

