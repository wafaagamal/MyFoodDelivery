using System;
using System.Threading.Tasks;

namespace OrderingSvc.Domain.Services;

/// <summary>
/// Domain service interface for cart operations (Redis-based).
/// Implemented in Infrastructure layer.
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
