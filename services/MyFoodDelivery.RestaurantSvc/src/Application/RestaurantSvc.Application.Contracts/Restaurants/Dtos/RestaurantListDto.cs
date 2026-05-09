using System;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

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
