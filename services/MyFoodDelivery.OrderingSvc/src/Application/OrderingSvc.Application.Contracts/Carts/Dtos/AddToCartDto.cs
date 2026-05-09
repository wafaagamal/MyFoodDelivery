using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Carts.Dtos;

public class AddToCartDto
{
    [Required]
    public Guid RestaurantId { get; set; }

    [Required]
    public Guid MenuItemId { get; set; }

    [StringLength(200)]
    public string? MenuItemName { get; set; }

    [StringLength(500)]
    public string? MenuItemImageUrl { get; set; }

    [Range(0, 9999.99)]
    public decimal UnitPrice { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; } = 1;

    [StringLength(500)]
    public string? SpecialInstructions { get; set; }

    public List<Guid>? SelectedOptionIds { get; set; }
}

