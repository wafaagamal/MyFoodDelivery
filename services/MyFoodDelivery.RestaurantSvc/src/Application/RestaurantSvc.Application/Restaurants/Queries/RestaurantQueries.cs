using System;
using System.Collections.Generic;
using MediatR;

namespace RestaurantSvc.Application.Restaurants.Queries;

#region Queries

public record GetRestaurantByIdQuery(Guid RestaurantId) : IRequest<RestaurantDetailDto?>;

public record GetRestaurantsByOwnerQuery(Guid OwnerId) : IRequest<List<RestaurantListDto>>;

public record SearchRestaurantsQuery(
    string? SearchTerm,
    string? CuisineType,
    double? Latitude,
    double? Longitude,
    double? RadiusKm,
    bool? OpenNow,
    decimal? MaxDeliveryFee,
    int SkipCount = 0,
    int MaxResultCount = 20) : IRequest<PagedResultDto<RestaurantListDto>>;

public record GetMenuQuery(Guid RestaurantId) : IRequest<MenuDto?>;

public record GetMenuItemQuery(Guid RestaurantId, Guid MenuItemId) : IRequest<MenuItemDto?>;

public record GetNearbyRestaurantsQuery(
    double Latitude,
    double Longitude,
    double RadiusKm = 5,
    int MaxResults = 20) : IRequest<List<NearbyRestaurantDto>>;

public record GetPopularRestaurantsQuery(
    string? City,
    int MaxResults = 10) : IRequest<List<RestaurantListDto>>;

public record GetRestaurantOrdersQuery(
    Guid RestaurantId,
    string? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    int SkipCount = 0,
    int MaxResultCount = 50) : IRequest<PagedResultDto<RestaurantOrderDto>>;

#endregion

#region DTOs

public record RestaurantDetailDto(
    Guid Id,
    string Name,
    string Description,
    string CuisineType,
    string PhoneNumber,
    string Email,
    RestaurantAddressDto Address,
    string? LogoUrl,
    string? BannerUrl,
    decimal MinimumOrderAmount,
    decimal DeliveryFee,
    int EstimatedDeliveryMinutes,
    decimal AverageRating,
    int TotalRatings,
    int TotalOrders,
    bool IsOpen,
    bool AcceptingOrders,
    List<OpeningHoursDto> OpeningHours,
    List<MenuCategoryDto> Categories);

public record RestaurantListDto(
    Guid Id,
    string Name,
    string CuisineType,
    string? LogoUrl,
    decimal DeliveryFee,
    int EstimatedDeliveryMinutes,
    decimal AverageRating,
    int TotalRatings,
    bool IsOpen,
    double? DistanceKm);

public record NearbyRestaurantDto(
    Guid Id,
    string Name,
    string CuisineType,
    string? LogoUrl,
    decimal DeliveryFee,
    int EstimatedDeliveryMinutes,
    decimal AverageRating,
    bool IsOpen,
    double DistanceKm,
    double Latitude,
    double Longitude);

public record RestaurantAddressDto(
    string Street,
    string BuildingNumber,
    string City,
    string? District,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude,
    string FullAddress);

public record MenuDto(
    Guid RestaurantId,
    string RestaurantName,
    List<MenuCategoryDto> Categories);

public record MenuCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    List<MenuItemDto> Items);

public record MenuItemDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountedPrice,
    string? ImageUrl,
    int PreparationTimeMinutes,
    bool IsVegetarian,
    bool IsVegan,
    bool IsGlutenFree,
    bool IsSpicy,
    List<string> Allergens,
    bool IsAvailable,
    bool IsFeatured);

public record OpeningHoursDto(
    DayOfWeek Day,
    TimeSpan OpenTime,
    TimeSpan CloseTime,
    bool IsClosed);

public record RestaurantOrderDto(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    List<OrderItemDto> Items,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? ReadyAt,
    string? SpecialInstructions);

public record OrderItemDto(
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string? SpecialInstructions);

public record PagedResultDto<T>(
    List<T> Items,
    int TotalCount);

#endregion
