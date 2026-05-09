using System;

namespace OrderingSvc.Application.Contracts.Carts.Dtos;

public class CartItemOptionDto
{
    public Guid OptionId { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
}

