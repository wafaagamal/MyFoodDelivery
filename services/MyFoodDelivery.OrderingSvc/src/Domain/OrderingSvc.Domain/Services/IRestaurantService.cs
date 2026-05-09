using System;
using System.Threading.Tasks;

namespace OrderingSvc.Domain.Services;

/// <summary>
/// Domain service interface for cross-service restaurant data access.
/// Implemented in Infrastructure layer via HTTP client to RestaurantSvc.
/// </summary>
public interface IRestaurantService
{
    Task<MenuItemInfo?> GetMenuItemAsync(Guid restaurantId, Guid menuItemId);
    Task<RestaurantInfo?> GetRestaurantAsync(Guid restaurantId);
}

public class MenuItemInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}

public class RestaurantInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal DeliveryFee { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public bool IsOpen { get; set; }
}
