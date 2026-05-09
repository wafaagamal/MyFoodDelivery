using System;
using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Payments.Dtos;

public class ProcessRefundDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Range(0.01, 100000)]
    public decimal? Amount { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }
}

