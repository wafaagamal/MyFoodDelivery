using System.ComponentModel.DataAnnotations;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class UpdateRestaurantDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = default!;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string CuisineType { get; set; } = default!;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Range(0, 10000)]
    public decimal MinimumOrderAmount { get; set; }

    [Range(0, 1000)]
    public decimal DeliveryFee { get; set; }

    [Range(10, 180)]
    public int EstimatedDeliveryMinutes { get; set; }
}
