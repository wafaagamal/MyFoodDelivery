using System;
using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Carts.Dtos;

public class UpdateCartItemDto
{
    [Required]
    public Guid MenuItemId { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }

    [StringLength(500)]
    public string? SpecialInstructions { get; set; }
}

