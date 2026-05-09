using System.ComponentModel.DataAnnotations;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class CreateRestaurantAddressDto
{
    [Required]
    [StringLength(200)]
    public string Street { get; set; } = default!;

    [Required]
    [StringLength(20)]
    public string BuildingNumber { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = default!;

    [StringLength(100)]
    public string? District { get; set; }

    [Required]
    [StringLength(20)]
    public string PostalCode { get; set; } = default!;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = default!;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }
}
