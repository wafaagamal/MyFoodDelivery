using System;
using System.Collections.Generic;

namespace OrderingSvc.Application.Contracts.Carts.Dtos;

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

