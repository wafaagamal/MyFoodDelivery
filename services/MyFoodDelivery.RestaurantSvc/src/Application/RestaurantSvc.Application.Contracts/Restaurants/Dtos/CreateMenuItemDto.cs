using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class CreateMenuItemDto
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = default!;

    [StringLength(1000)]
    public string Description { get; set; } = default!;

    [Required]
    [Range(0.01, 10000)]
    public decimal Price { get; set; }

    [Url]
    public string? ImageUrl { get; set; }

    [Range(1, 180)]
    public int PreparationTimeMinutes { get; set; } = 15;

    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsSpicy { get; set; }
    public List<string>? Allergens { get; set; }
}
