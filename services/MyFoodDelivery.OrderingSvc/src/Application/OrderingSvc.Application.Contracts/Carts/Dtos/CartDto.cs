using System;
using System.Collections.Generic;

namespace OrderingSvc.Application.Contracts.Carts.Dtos;

public class CartDto
{
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = default!;
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Tip { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Total { get; set; }
    public string? PromoCode { get; set; }
    public decimal Discount { get; set; }
    public DateTime UpdatedAt { get; set; }
}

