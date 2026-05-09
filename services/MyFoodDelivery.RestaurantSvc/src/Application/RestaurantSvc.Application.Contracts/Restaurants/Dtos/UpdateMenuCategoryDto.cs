using System.ComponentModel.DataAnnotations;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class UpdateMenuCategoryDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
}
