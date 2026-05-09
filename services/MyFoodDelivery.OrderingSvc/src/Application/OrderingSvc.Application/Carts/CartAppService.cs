using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrderingSvc.Application.Contracts.Carts;
using OrderingSvc.Application.Contracts.Carts.Dtos;
using OrderingSvc.Domain.Services;
using Volo.Abp.Application.Services;

namespace OrderingSvc.Application.Carts;

/// <summary>
/// ABP Application Service for cart management.
/// Uses Redis for cart storage.
/// </summary>
[Authorize]
public class CartAppService : ApplicationService, ICartAppService
{
    private readonly ICartService _cartService;

    public CartAppService(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<CartDto?> GetAsync()
    {
        var customerId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");
        var cart = await _cartService.GetCartAsync(customerId);
        if (cart == null)
        {
            return null;
        }

        return MapToCartDto(cart);
    }

    public async Task<CartDto> AddItemAsync(AddToCartDto input)
    {
        var customerId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");

        var cartItem = new CartItem
        {
            MenuItemId = input.MenuItemId,
            Name = input.MenuItemName ?? "Menu Item",
            ImageUrl = input.MenuItemImageUrl,
            UnitPrice = input.UnitPrice,
            Quantity = input.Quantity,
            SpecialInstructions = input.SpecialInstructions,
            SelectedOptions = new()
        };

        await _cartService.AddItemAsync(customerId, input.RestaurantId, cartItem);
        
        var cart = await _cartService.GetCartAsync(customerId);
        return MapToCartDto(cart!);
    }

    public async Task<CartDto> UpdateItemAsync(UpdateCartItemDto input)
    {
        var customerId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");
        
        await _cartService.UpdateItemQuantityAsync(customerId, input.MenuItemId, input.Quantity);
        
        var cart = await _cartService.GetCartAsync(customerId);
        return MapToCartDto(cart!);
    }

    public async Task<CartDto> RemoveItemAsync(RemoveFromCartDto input)
    {
        var customerId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");
        
        await _cartService.RemoveItemAsync(customerId, input.MenuItemId);
        
        var cart = await _cartService.GetCartAsync(customerId);
        if (cart == null)
        {
            return new CartDto { CustomerId = customerId };
        }
        return MapToCartDto(cart);
    }

    public async Task ClearAsync()
    {
        var customerId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");
        await _cartService.ClearCartAsync(customerId);
    }

    public async Task<CartDto> ApplyPromoCodeAsync(ApplyPromoCodeDto input)
    {
        var customerId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");
        // TODO: Implement promo code logic
        var cart = await _cartService.GetCartAsync(customerId);
        return MapToCartDto(cart!);
    }

    public async Task<CartDto> RemovePromoCodeAsync()
    {
        var customerId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");
        // TODO: Implement promo code logic
        var cart = await _cartService.GetCartAsync(customerId);
        return MapToCartDto(cart!);
    }

    public async Task<CartDto> SetTipAsync(SetTipDto input)
    {
        var customerId = CurrentUser.Id ?? throw new InvalidOperationException("User not authenticated");
        // TODO: Implement tip logic
        var cart = await _cartService.GetCartAsync(customerId);
        return MapToCartDto(cart!);
    }

    private static CartDto MapToCartDto(Cart cart)
    {
        return new CartDto
        {
            CustomerId = cart.CustomerId,
            RestaurantId = cart.RestaurantId,
            RestaurantName = string.Empty, // TODO: Fetch from restaurant service
            Items = cart.Items.Select(i => new CartItemDto
            {
                MenuItemId = i.MenuItemId,
                Name = i.Name,
                ImageUrl = i.ImageUrl,
                UnitPrice = i.UnitPrice,
                Quantity = i.Quantity,
                SpecialInstructions = i.SpecialInstructions,
                SelectedOptions = i.SelectedOptions.Select(o => new CartItemOptionDto
                {
                    OptionId = o.OptionId,
                    Name = o.Name,
                    Price = o.Price
                }).ToList(),
                TotalPrice = i.UnitPrice * i.Quantity
            }).ToList(),
            Subtotal = cart.Subtotal,
            DeliveryFee = cart.DeliveryFee,
            Tip = cart.Tip,
            ServiceFee = cart.ServiceFee,
            Total = cart.Subtotal + cart.DeliveryFee + cart.Tip + cart.ServiceFee - cart.Discount,
            PromoCode = cart.PromoCode,
            Discount = cart.Discount,
            UpdatedAt = cart.UpdatedAt
        };
    }
}
