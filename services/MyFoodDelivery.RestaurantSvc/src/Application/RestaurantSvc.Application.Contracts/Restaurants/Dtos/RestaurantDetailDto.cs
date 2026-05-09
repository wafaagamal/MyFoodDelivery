using System;
using System.Collections.Generic;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

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
