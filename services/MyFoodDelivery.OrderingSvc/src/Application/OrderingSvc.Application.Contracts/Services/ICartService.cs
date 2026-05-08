using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSvc.Application.Services;

/// <summary>
/// Cart domain model for Redis storage
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

/// <summary>
/// Cart item model
/// </summary>
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

/// <summary>
/// Cart item option
/// </summary>
public class CartItemOption
{
    public Guid OptionId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}

/// <summary>
/// Cart totals calculation result
/// </summary>
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

/// <summary>
/// Internal cart service interface for Redis-based cart operations
/// </summary>
public interface ICartService
{
    Task<Cart?> GetCartAsync(Guid customerId);
    Task AddItemAsync(Guid customerId, Guid restaurantId, CartItem item);
    Task UpdateItemQuantityAsync(Guid customerId, Guid menuItemId, int quantity);
    Task RemoveItemAsync(Guid customerId, Guid menuItemId);
    Task ClearCartAsync(Guid customerId);
    Task<CartTotals> CalculateTotalsAsync(Guid customerId, string? promoCode);
}
