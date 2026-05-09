using System;
using System.Collections.Generic;
using MediatR;
using RestaurantSvc.Application.Contracts.Restaurants.Dtos;

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
    int MaxResultCount = 20) : IRequest<RestaurantPagedResult<RestaurantListDto>>;

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
    int MaxResultCount = 50) : IRequest<RestaurantPagedResult<RestaurantOrderDto>>;

#endregion

