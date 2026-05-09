using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Carts.Dtos;

public class ApplyPromoCodeDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = default!;
}

