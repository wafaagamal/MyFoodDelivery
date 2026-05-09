using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using OrderingSvc.Domain.Services;
using StackExchange.Redis;

namespace OrderingSvc.Infrastructure.Services;

/// <summary>
/// Redis-based cart service implementation
/// </summary>
public class RedisCartService : ICartService
{
    private readonly IDatabase _redis;
    private readonly TimeSpan _cartExpiry = TimeSpan.FromDays(7);

    public RedisCartService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    private static string GetCartKey(Guid customerId) => $"cart:{customerId}";

    public async Task<Cart?> GetCartAsync(Guid customerId)
    {
        var key = GetCartKey(customerId);
        var data = await _redis.StringGetAsync(key);
        
        if (data.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Cart>(data!);
    }

    public async Task AddItemAsync(Guid customerId, Guid restaurantId, CartItem item)
    {
        var key = GetCartKey(customerId);
        var cart = await GetCartAsync(customerId);

        if (cart == null)
        {
            cart = new Cart
            {
                CustomerId = customerId,
                RestaurantId = restaurantId,
                Items = new List<CartItem> { item },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
        else
        {
            // If different restaurant, clear cart first
            if (cart.RestaurantId != restaurantId)
            {
                cart = new Cart
                {
                    CustomerId = customerId,
                    RestaurantId = restaurantId,
                    Items = new List<CartItem> { item },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                // Check if item already exists
                var existing = cart.Items.FirstOrDefault(i => i.MenuItemId == item.MenuItemId);
                if (existing != null)
                {
                    existing.Quantity += item.Quantity;
                    if (!string.IsNullOrEmpty(item.SpecialInstructions))
                        existing.SpecialInstructions = item.SpecialInstructions;
                }
                else
                {
                    cart.Items.Add(item);
                }
                cart.UpdatedAt = DateTime.UtcNow;
            }
        }

        await SaveCartAsync(key, cart);
    }

    public async Task UpdateItemQuantityAsync(Guid customerId, Guid menuItemId, int quantity)
    {
        var cart = await GetCartAsync(customerId);
        if (cart == null) return;

        var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (item == null) return;

        if (quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        if (cart.Items.Count == 0)
        {
            await ClearCartAsync(customerId);
        }
        else
        {
            await SaveCartAsync(GetCartKey(customerId), cart);
        }
    }

    public async Task RemoveItemAsync(Guid customerId, Guid menuItemId)
    {
        await UpdateItemQuantityAsync(customerId, menuItemId, 0);
    }

    public async Task ClearCartAsync(Guid customerId)
    {
        var key = GetCartKey(customerId);
        await _redis.KeyDeleteAsync(key);
    }

    public async Task<CartTotals> CalculateTotalsAsync(Guid customerId, string? promoCode)
    {
        var cart = await GetCartAsync(customerId);
        
        if (cart == null)
        {
            return new CartTotals();
        }

        var subtotal = cart.Subtotal;
        var deliveryFee = cart.DeliveryFee;
        var serviceFee = Math.Round(subtotal * 0.05m, 2); // 5% service fee
        decimal discount = 0;

        // TODO: Validate promo code if provided
        // This would call IPromoCodeService

        return new CartTotals
        {
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            ServiceFee = serviceFee,
            Discount = discount,
            PromoCode = promoCode
        };
    }

    private async Task SaveCartAsync(string key, Cart cart)
    {
        cart.Subtotal = cart.Items.Sum(i => i.UnitPrice * i.Quantity);
        var json = JsonSerializer.Serialize(cart);
        await _redis.StringSetAsync(key, json, _cartExpiry);
    }
}
