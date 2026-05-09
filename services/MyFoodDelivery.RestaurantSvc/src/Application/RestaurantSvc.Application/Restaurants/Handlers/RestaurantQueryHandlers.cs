using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using RestaurantSvc.Application.Contracts.Restaurants.Dtos;
using RestaurantSvc.Application.Restaurants.Queries;
using RestaurantSvc.Domain.Restaurants;

namespace RestaurantSvc.Application.Restaurants.Handlers;

public class RestaurantQueryHandlers :
    IRequestHandler<GetRestaurantByIdQuery, RestaurantDetailDto?>,
    IRequestHandler<GetRestaurantsByOwnerQuery, List<RestaurantListDto>>,
    IRequestHandler<SearchRestaurantsQuery, RestaurantPagedResult<RestaurantListDto>>,
    IRequestHandler<GetMenuQuery, MenuDto?>,
    IRequestHandler<GetNearbyRestaurantsQuery, List<NearbyRestaurantDto>>
{
    private readonly IMongoCollection<Restaurant> _collection;

    public RestaurantQueryHandlers(IMongoDatabase database)
    {
        _collection = database.GetCollection<Restaurant>("restaurants");
    }

    public async Task<RestaurantDetailDto?> Handle(GetRestaurantByIdQuery request, CancellationToken cancellationToken)
    {
        var restaurant = await _collection
            .Find(r => r.Id == request.RestaurantId && r.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (restaurant == null) return null;

        return MapToDetailDto(restaurant);
    }

    public async Task<List<RestaurantListDto>> Handle(GetRestaurantsByOwnerQuery request, CancellationToken cancellationToken)
    {
        var restaurants = await _collection
            .Find(r => r.OwnerId == request.OwnerId)
            .ToListAsync(cancellationToken);

        return restaurants.Select(r => MapToListDto(r, null)).ToList();
    }

    public async Task<RestaurantPagedResult<RestaurantListDto>> Handle(SearchRestaurantsQuery request, CancellationToken cancellationToken)
    {
        var filter = Builders<Restaurant>.Filter.Eq(r => r.IsActive, true);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            filter &= Builders<Restaurant>.Filter.Or(
                Builders<Restaurant>.Filter.Regex(r => r.Name, new MongoDB.Bson.BsonRegularExpression(request.SearchTerm, "i")),
                Builders<Restaurant>.Filter.Regex(r => r.CuisineType, new MongoDB.Bson.BsonRegularExpression(request.SearchTerm, "i"))
            );
        }

        if (!string.IsNullOrEmpty(request.CuisineType))
        {
            filter &= Builders<Restaurant>.Filter.Eq(r => r.CuisineType, request.CuisineType);
        }

        if (request.OpenNow == true)
        {
            filter &= Builders<Restaurant>.Filter.Eq(r => r.IsOpen, true);
        }

        if (request.MaxDeliveryFee.HasValue)
        {
            filter &= Builders<Restaurant>.Filter.Lte(r => r.DeliveryFee, request.MaxDeliveryFee.Value);
        }

        var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var restaurants = await _collection
            .Find(filter)
            .SortByDescending(r => r.AverageRating)
            .Skip(request.SkipCount)
            .Limit(request.MaxResultCount)
            .ToListAsync(cancellationToken);

        var items = restaurants.Select(r =>
        {
            double? distance = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                distance = CalculateDistance(
                    request.Latitude.Value, request.Longitude.Value,
                    r.Address.Latitude, r.Address.Longitude);
            }
            return MapToListDto(r, distance);
        }).ToList();

        // Filter by radius if specified
        if (request.RadiusKm.HasValue && request.Latitude.HasValue)
        {
            items = items.Where(i => i.DistanceKm <= request.RadiusKm.Value).ToList();
        }

        return new RestaurantPagedResult<RestaurantListDto>(items, (int)totalCount);
    }

    public async Task<MenuDto?> Handle(GetMenuQuery request, CancellationToken cancellationToken)
    {
        var restaurant = await _collection
            .Find(r => r.Id == request.RestaurantId && r.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (restaurant == null) return null;

        var categories = restaurant.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new MenuCategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.DisplayOrder,
                restaurant.MenuItems
                    .Where(m => m.CategoryId == c.Id && m.IsActive)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => MapToMenuItemDto(m))
                    .ToList()))
            .ToList();

        return new MenuDto(restaurant.Id, restaurant.Name, categories);
    }

    public async Task<List<NearbyRestaurantDto>> Handle(GetNearbyRestaurantsQuery request, CancellationToken cancellationToken)
    {
        // MongoDB geospatial query - assumes 2dsphere index on Address.Latitude/Longitude
        var restaurants = await _collection
            .Find(r => r.IsActive && r.IsOpen)
            .Limit(100)
            .ToListAsync(cancellationToken);

        var nearby = restaurants
            .Select(r =>
            {
                var distance = CalculateDistance(
                    request.Latitude, request.Longitude,
                    r.Address.Latitude, r.Address.Longitude);

                return new NearbyRestaurantDto(
                    r.Id,
                    r.Name,
                    r.CuisineType,
                    r.LogoUrl,
                    r.DeliveryFee,
                    r.EstimatedDeliveryMinutes,
                    r.AverageRating,
                    r.IsOpen,
                    Math.Round(distance, 2),
                    r.Address.Latitude,
                    r.Address.Longitude);
            })
            .Where(r => r.DistanceKm <= request.RadiusKm)
            .OrderBy(r => r.DistanceKm)
            .Take(request.MaxResults)
            .ToList();

        return nearby;
    }

    #region Mapping Helpers

    private static RestaurantDetailDto MapToDetailDto(Restaurant r)
    {
        return new RestaurantDetailDto(
            r.Id,
            r.Name,
            r.Description,
            r.CuisineType,
            r.PhoneNumber,
            r.Email,
            new RestaurantAddressDto(
                r.Address.Street,
                r.Address.BuildingNumber,
                r.Address.City,
                r.Address.District,
                r.Address.PostalCode,
                r.Address.Country,
                r.Address.Latitude,
                r.Address.Longitude,
                r.Address.GetFullAddress()),
            r.LogoUrl,
            r.BannerUrl,
            r.MinimumOrderAmount,
            r.DeliveryFee,
            r.EstimatedDeliveryMinutes,
            r.AverageRating,
            r.TotalRatings,
            r.TotalOrders,
            r.IsOpen,
            r.AcceptingOrders,
            r.OpeningHours.Select(h => new OpeningHoursDto(h.Day, h.OpenTime, h.CloseTime, h.IsClosed)).ToList(),
            r.Categories.Select(c => new MenuCategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.DisplayOrder,
                r.MenuItems.Where(m => m.CategoryId == c.Id && m.IsActive)
                    .Select(m => MapToMenuItemDto(m))
                    .ToList())).ToList());
    }

    private static RestaurantListDto MapToListDto(Restaurant r, double? distance)
    {
        return new RestaurantListDto(
            r.Id,
            r.Name,
            r.CuisineType,
            r.LogoUrl,
            r.DeliveryFee,
            r.EstimatedDeliveryMinutes,
            r.AverageRating,
            r.TotalRatings,
            r.IsOpen,
            distance.HasValue ? Math.Round(distance.Value, 2) : null);
    }

    private static MenuItemDto MapToMenuItemDto(MenuItem m)
    {
        return new MenuItemDto(
            m.Id,
            m.CategoryId,
            m.Name,
            m.Description,
            m.Price,
            m.DiscountedPrice,
            m.ImageUrl,
            m.PreparationTimeMinutes,
            m.IsVegetarian,
            m.IsVegan,
            m.IsGlutenFree,
            m.IsSpicy,
            m.Allergens,
            m.IsAvailable,
            m.IsFeatured);
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double deg) => deg * Math.PI / 180;

    #endregion
}
