using System;
using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class RateOrderDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Review { get; set; }

    [Range(1, 5)]
    public int? DeliveryRating { get; set; }

    [StringLength(1000)]
    public string? DeliveryReview { get; set; }
}

