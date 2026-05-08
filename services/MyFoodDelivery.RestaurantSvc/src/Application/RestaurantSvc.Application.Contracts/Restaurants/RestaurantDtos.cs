using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace RestaurantSvc.Application.Contracts.Restaurants;

#region Restaurant DTOs

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

public class RestaurantListDto : EntityDto<Guid>
{
    public string Name { get; set; } = default!;
    public string CuisineType { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public decimal DeliveryFee { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public bool IsOpen { get; set; }
    public double? DistanceKm { get; set; }
}

public class NearbyRestaurantDto : EntityDto<Guid>
{
    public string Name { get; set; } = default!;
    public string CuisineType { get; set; } = default!;
    public string? LogoUrl { get; set; }
    public decimal DeliveryFee { get; set; }
    public int EstimatedDeliveryMinutes { get; set; }
    public decimal AverageRating { get; set; }
    public bool IsOpen { get; set; }
    public double DistanceKm { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class RestaurantAddressDto
{
    public string Street { get; set; } = default!;
    public string BuildingNumber { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string FullAddress { get; set; } = default!;
}

public class OpeningHoursDto
{
    public DayOfWeek Day { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

#endregion

#region Create/Update DTOs

public class CreateRestaurantDto
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

    [Required]
    public CreateRestaurantAddressDto Address { get; set; } = default!;

    [Range(0, 10000)]
    public decimal MinimumOrderAmount { get; set; }

    [Range(0, 1000)]
    public decimal DeliveryFee { get; set; }

    [Range(10, 180)]
    public int EstimatedDeliveryMinutes { get; set; } = 45;
}

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

public class SetOpeningHoursDto
{
    [Required]
    public List<OpeningHoursDto> Hours { get; set; } = new();
}

#endregion

#region Menu DTOs

public class MenuDto
{
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = default!;
    public List<MenuCategoryDto> Categories { get; set; } = new();
}

public class MenuCategoryDto : EntityDto<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public List<MenuItemDto> Items { get; set; } = new();
}

public class MenuItemDto : EntityDto<Guid>
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsSpicy { get; set; }
    public List<string> Allergens { get; set; } = new();
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
}

public class CreateMenuCategoryDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
}

public class UpdateMenuCategoryDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    [StringLength(500)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
}

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

public class UpdateMenuItemDto
{
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
    public int PreparationTimeMinutes { get; set; }

    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsSpicy { get; set; }
    public List<string>? Allergens { get; set; }
}

public class SetMenuItemAvailabilityDto
{
    public bool IsAvailable { get; set; }
}

public class UpdateMenuItemStockDto
{
    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}

#endregion

#region Search DTOs

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

#endregion
