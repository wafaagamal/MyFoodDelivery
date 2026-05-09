using System;
using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Carts.Dtos;

public class RemoveFromCartDto
{
    [Required]
    public Guid MenuItemId { get; set; }
}

