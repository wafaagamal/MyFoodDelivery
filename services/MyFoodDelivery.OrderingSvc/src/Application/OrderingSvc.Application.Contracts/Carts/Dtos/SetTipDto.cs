using System.ComponentModel.DataAnnotations;

namespace OrderingSvc.Application.Contracts.Carts.Dtos;

public class SetTipDto
{
    [Range(0, 1000)]
    public decimal Amount { get; set; }
}

