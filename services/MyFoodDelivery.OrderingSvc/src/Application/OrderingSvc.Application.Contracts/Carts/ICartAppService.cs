using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace OrderingSvc.Application.Contracts.Carts;

/// <summary>
/// ABP Application Service interface for cart management.
/// </summary>
public interface ICartAppService : IApplicationService
{
    /// <summary>
    /// Get current user's cart
    /// </summary>
    Task<CartDto?> GetAsync();

    /// <summary>
    /// Add item to cart
    /// </summary>
    Task<CartDto> AddItemAsync(AddToCartDto input);

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    Task<CartDto> UpdateItemAsync(UpdateCartItemDto input);

    /// <summary>
    /// Remove item from cart
    /// </summary>
    Task<CartDto> RemoveItemAsync(RemoveFromCartDto input);

    /// <summary>
    /// Clear entire cart
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Apply promo code to cart
    /// </summary>
    Task<CartDto> ApplyPromoCodeAsync(ApplyPromoCodeDto input);

    /// <summary>
    /// Remove promo code from cart
    /// </summary>
    Task<CartDto> RemovePromoCodeAsync();

    /// <summary>
    /// Set tip amount
    /// </summary>
    Task<CartDto> SetTipAsync(SetTipDto input);
}
