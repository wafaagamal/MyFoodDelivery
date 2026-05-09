using System.ComponentModel.DataAnnotations;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class UpdateMenuItemStockDto
{
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}
