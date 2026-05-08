using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace OrderingSvc.Application.Contracts.Carts;

#region Cart DTOs

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

public class CartItemDto
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<CartItemOptionDto> SelectedOptions { get; set; } = new();
    public decimal TotalPrice { get; set; }
}

public class CartItemOptionDto
{
    public Guid OptionId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}

#endregion

#region Cart Commands

public class AddToCartDto
{
    [Required]
    public Guid RestaurantId { get; set; }

    [Required]
    public Guid MenuItemId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;

    [StringLength(500)]
    public string? SpecialInstructions { get; set; }

    public List<Guid>? SelectedOptionIds { get; set; }
}

public class UpdateCartItemDto
{
    [Required]
    public Guid MenuItemId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }

    [StringLength(500)]
    public string? SpecialInstructions { get; set; }
}

public class RemoveFromCartDto
{
    [Required]
    public Guid MenuItemId { get; set; }
}

public class ApplyPromoCodeDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;
}

public class SetTipDto
{
    [Range(0, 1000)]
    public decimal Amount { get; set; }
}

#endregion
