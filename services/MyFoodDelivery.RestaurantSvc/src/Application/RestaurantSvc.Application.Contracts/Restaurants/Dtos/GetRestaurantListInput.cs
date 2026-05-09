using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class GetRestaurantListInput : PagedAndSortedResultRequestDto
{
    public string? SearchTerm { get; set; }
    public string? CuisineType { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public bool? OpenNow { get; set; }
    public decimal? MaxDeliveryFee { get; set; }
}
