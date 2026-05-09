using System;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class OrderItemDto
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SpecialInstructions { get; set; }
}

