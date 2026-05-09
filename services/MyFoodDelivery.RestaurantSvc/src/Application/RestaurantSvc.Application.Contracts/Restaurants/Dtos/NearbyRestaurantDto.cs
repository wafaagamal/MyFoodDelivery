using System;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

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
