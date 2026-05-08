using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderingSvc.Domain.Services;

/// <summary>
/// Domain service interface for cart operations (Redis-based).
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Get cart for a customer
    /// </summary>
    Task<Cart?> GetCartAsync(Guid customerId);

    /// <summary>
    /// Add item to cart
    /// </summary>
    Task<Cart> AddItemAsync(
        Guid customerId,
        Guid restaurantId,
        Guid menuItemId,
        string name,
        decimal price,
        string? imageUrl,
        int quantity,
        string? specialInstructions);

    /// <summary>
    /// Update item quantity
    /// </summary>
    Task<Cart> UpdateItemQuantityAsync(
        Guid customerId,
        Guid menuItemId,
        int quantity,
        string? specialInstructions);

    /// <summary>
    /// Remove item from cart
    /// </summary>
    Task<Cart> RemoveItemAsync(Guid customerId, Guid menuItemId);

    /// <summary>
    /// Clear entire cart
    /// </summary>
    Task ClearCartAsync(Guid customerId);

    /// <summary>
    /// Apply promo code
    /// </summary>
    Task<Cart> ApplyPromoCodeAsync(Guid customerId, string code);

    /// <summary>
    /// Remove promo code
    /// </summary>
    Task<Cart> RemovePromoCodeAsync(Guid customerId);

    /// <summary>
    /// Set tip amount
    /// </summary>
    Task<Cart> SetTipAsync(Guid customerId, decimal amount);
}

/// <summary>
/// Domain service interface for payment processing.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Create a payment intent (Stripe)
    /// </summary>
    Task<PaymentIntent> CreatePaymentIntentAsync(Guid orderId, decimal amount);

    /// <summary>
    /// Process a payment
    /// </summary>
    Task<PaymentResult> ProcessPaymentAsync(Guid orderId, decimal amount, string paymentMethodId);

    /// <summary>
    /// Confirm a payment
    /// </summary>
    Task<PaymentResult> ConfirmPaymentAsync(string paymentIntentId, string paymentMethodId);

    /// <summary>
    /// Process a refund
    /// </summary>
    Task<RefundResult> ProcessRefundAsync(Guid orderId, decimal amount, string? reason);

    /// <summary>
    /// Handle webhook from payment provider
    /// </summary>
    Task HandleWebhookAsync(string payload, string signature);
}

/// <summary>
/// Domain service interface for restaurant info (cross-service call).
/// </summary>
public interface IRestaurantService
{
    /// <summary>
    /// Get menu item details
    /// </summary>
    Task<MenuItemInfo> GetMenuItemAsync(Guid restaurantId, Guid menuItemId);

    /// <summary>
    /// Get restaurant info
    /// </summary>
    Task<RestaurantInfo> GetRestaurantAsync(Guid restaurantId);
}

#region Domain Models

public class Cart
{
    public Guid CustomerId { get; set; }
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = default!;
    public List<CartItem> Items { get; set; } = new();
    public decimal DeliveryFee { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Tip { get; set; }
    public decimal Discount { get; set; }
    public string? PromoCode { get; set; }
    public DateTime UpdatedAt { get; set; }

    public decimal GetSubtotal() => Items.Sum(i => i.GetTotalPrice());
    public decimal GetTotal() => GetSubtotal() + DeliveryFee + ServiceFee + Tip - Discount;
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

    public decimal GetTotalPrice() => (UnitPrice + SelectedOptions.Sum(o => o.Price)) * Quantity;
}

public class CartItemOption
{
    public Guid OptionId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}

public class PaymentIntent
{
    public string PaymentIntentId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string Status { get; set; } = default!;
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RefundResult
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class MenuItemInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}

public class RestaurantInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal DeliveryFee { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public bool IsOpen { get; set; }
}

#endregion
