using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class CancelOrderDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = default!;
}

