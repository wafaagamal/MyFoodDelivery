using System.ComponentModel.DataAnnotations;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class GetNearbyRestaurantsInput
{
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Range(1, 50)]
    public double RadiusKm { get; set; } = 5;

    [Range(1, 100)]
    public int MaxResults { get; set; } = 20;
}
