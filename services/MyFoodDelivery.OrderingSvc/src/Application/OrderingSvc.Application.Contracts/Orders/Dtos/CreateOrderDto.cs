using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class CreateOrderDto
{
    [Required]
    public DeliveryAddressDto DeliveryAddress { get; set; } = default!;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public string? PaymentMethodId { get; set; }

    [StringLength(256)]
    public string? RestaurantName { get; set; }
}

