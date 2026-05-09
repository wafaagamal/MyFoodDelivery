using System;
using System.Collections.Generic;
using System.Linq;

namespace OrderingSvc.Domain.Services;

/// <summary>
/// Cart aggregate — Redis/session-level model, not a DB entity.
/// </summary>
public class Cart
{
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Tip { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Discount { get; set; }
    public string? PromoCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CartItem
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<CartItemOption> SelectedOptions { get; set; } = new();
}

public class CartItemOption
{
    public Guid OptionId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}

public class CartTotals
{
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Tip { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string? PromoCode { get; set; }
}
