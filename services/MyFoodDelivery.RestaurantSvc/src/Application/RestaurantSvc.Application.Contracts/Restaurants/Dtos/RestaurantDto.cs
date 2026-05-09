using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public class RestaurantDto : FullAuditedEntityDto<Guid>
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string CuisineType { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public RestaurantAddressDto Address { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int TotalOrders { get; set; }
    public bool IsActive { get; set; }
    public bool IsOpen { get; set; }
    public bool AcceptingOrders { get; set; }
    public List<OpeningHoursDto> OpeningHours { get; set; } = new();
    public List<MenuCategoryDto> Categories { get; set; } = new();
}
