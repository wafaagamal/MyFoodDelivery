using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RestaurantSvc.Application.Contracts.Permissions;
using RestaurantSvc.Application.Contracts.Restaurants;
using RestaurantSvc.Domain.Restaurants;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using MyFoodDelivery.Shared.Events;

namespace RestaurantSvc.Application.Restaurants;

/// <summary>
/// ABP Application Service for restaurant management.
/// </summary>
[Authorize]
public class RestaurantAppService : ApplicationService, IRestaurantAppService
{
    private readonly IRepository<Restaurant, Guid> _restaurantRepository;
    private readonly IDistributedEventBus _eventBus;

    public RestaurantAppService(
        IRepository<Restaurant, Guid> restaurantRepository,
        IDistributedEventBus eventBus)
    {
        _restaurantRepository = restaurantRepository;
        _eventBus = eventBus;
    }

    #region Restaurant CRUD

    [AllowAnonymous]
    public async Task<RestaurantDto> GetAsync(Guid id)
    {
        var restaurant = await _restaurantRepository.GetAsync(id);
        return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<RestaurantListDto>> GetListAsync(GetRestaurantListInput input)
    {
        var queryable = await _restaurantRepository.GetQueryableAsync();

        // Apply filters
        queryable = queryable.Where(r => r.IsActive);

        if (!string.IsNullOrEmpty(input.SearchTerm))
        {
            var term = input.SearchTerm.ToLower();
            queryable = queryable.Where(r =>
                r.Name.ToLower().Contains(term) ||
                r.CuisineType.ToLower().Contains(term));
        }

        if (!string.IsNullOrEmpty(input.CuisineType))
        {
            queryable = queryable.Where(r => r.CuisineType == input.CuisineType);
        }

        if (input.OpenNow == true)
        {
            queryable = queryable.Where(r => r.IsOpen);
        }

        if (input.MaxDeliveryFee.HasValue)
        {
            queryable = queryable.Where(r => r.DeliveryFee <= input.MaxDeliveryFee);
        }

        var totalCount = queryable.Count();

        // Apply sorting
        queryable = ApplySorting(queryable, input);

        // Apply paging
        queryable = queryable
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var restaurants = queryable.ToList();

        var dtos = restaurants.Select(r =>
        {
            var dto = ObjectMapper.Map<Restaurant, RestaurantListDto>(r);

            // Calculate distance if coordinates provided
            if (input.Latitude.HasValue && input.Longitude.HasValue)
            {
                dto.DistanceKm = CalculateDistance(
                    input.Latitude.Value, input.Longitude.Value,
                    r.Address.Latitude, r.Address.Longitude);
            }

            return dto;
        }).ToList();

        return new PagedResultDto<RestaurantListDto>(totalCount, dtos);
    }

    public async Task<List<RestaurantListDto>> GetByOwnerAsync()
    {
        var queryable = await _restaurantRepository.GetQueryableAsync();
        var restaurants = queryable
            .Where(r => r.OwnerId == CurrentUser.Id)
            .ToList();

        return ObjectMapper.Map<List<Restaurant>, List<RestaurantListDto>>(restaurants);
    }

    [AllowAnonymous]
    public async Task<List<NearbyRestaurantDto>> GetNearbyAsync(GetNearbyRestaurantsInput input)
    {
        var queryable = await _restaurantRepository.GetQueryableAsync();
        var restaurants = queryable
            .Where(r => r.IsActive && r.IsOpen)
            .ToList();

        var nearby = restaurants
            .Select(r =>
            {
                var distance = CalculateDistance(
                    input.Latitude, input.Longitude,
                    r.Address.Latitude, r.Address.Longitude);

                var dto = ObjectMapper.Map<Restaurant, NearbyRestaurantDto>(r);
                dto.DistanceKm = Math.Round(distance, 2);
                dto.Latitude = r.Address.Latitude;
                dto.Longitude = r.Address.Longitude;
                return dto;
            })
            .Where(r => r.DistanceKm <= input.RadiusKm)
            .OrderBy(r => r.DistanceKm)
            .Take(input.MaxResults)
            .ToList();

        return nearby;
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.Create)]
    public async Task<RestaurantDto> CreateAsync(CreateRestaurantDto input)
    {
        var address = new RestaurantAddress(
            input.Address.Street,
            input.Address.BuildingNumber,
            input.Address.City,
            input.Address.District,
            input.Address.PostalCode,
            input.Address.Country,
            input.Address.Latitude,
            input.Address.Longitude);

        var restaurant = new Restaurant(
            GuidGenerator.Create(),
            CurrentUser.Id!.Value,
            input.Name,
            input.Description,
            input.CuisineType,
            input.PhoneNumber,
            input.Email,
            address,
            input.MinimumOrderAmount,
            input.DeliveryFee,
            input.EstimatedDeliveryMinutes);

        await _restaurantRepository.InsertAsync(restaurant);

        return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.Edit)]
    public async Task<RestaurantDto> UpdateAsync(Guid id, UpdateRestaurantDto input)
    {
        var restaurant = await _restaurantRepository.GetAsync(id);

        await CheckOwnershipAsync(restaurant);

        restaurant.UpdateInfo(
            input.Name,
            input.Description,
            input.CuisineType,
            input.PhoneNumber,
            input.Email,
            input.MinimumOrderAmount,
            input.DeliveryFee,
            input.EstimatedDeliveryMinutes);

        await _restaurantRepository.UpdateAsync(restaurant);

        return ObjectMapper.Map<Restaurant, RestaurantDto>(restaurant);
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var restaurant = await _restaurantRepository.GetAsync(id);
        await CheckOwnershipAsync(restaurant);
        await _restaurantRepository.DeleteAsync(restaurant);
    }

    #endregion

    #region Restaurant Status

    [Authorize(RestaurantSvcPermissions.Restaurants.Edit)]
    public async Task OpenAsync(Guid id)
    {
        var restaurant = await _restaurantRepository.GetAsync(id);
        await CheckOwnershipAsync(restaurant);

        restaurant.Open();
        await _restaurantRepository.UpdateAsync(restaurant);

        await _eventBus.PublishAsync(new RestaurantStatusChangedEto(
            restaurant.Id,
            restaurant.IsOpen,
            restaurant.AcceptingOrders));
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.Edit)]
    public async Task CloseAsync(Guid id)
    {
        var restaurant = await _restaurantRepository.GetAsync(id);
        await CheckOwnershipAsync(restaurant);

        restaurant.Close();
        await _restaurantRepository.UpdateAsync(restaurant);

        await _eventBus.PublishAsync(new RestaurantStatusChangedEto(
            restaurant.Id,
            restaurant.IsOpen,
            restaurant.AcceptingOrders));
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.Edit)]
    public async Task PauseOrdersAsync(Guid id, string reason)
    {
        var restaurant = await _restaurantRepository.GetAsync(id);
        await CheckOwnershipAsync(restaurant);

        restaurant.PauseOrders(reason);
        await _restaurantRepository.UpdateAsync(restaurant);
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.Edit)]
    public async Task ResumeOrdersAsync(Guid id)
    {
        var restaurant = await _restaurantRepository.GetAsync(id);
        await CheckOwnershipAsync(restaurant);

        restaurant.ResumeOrders();
        await _restaurantRepository.UpdateAsync(restaurant);
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.Edit)]
    public async Task SetOpeningHoursAsync(Guid id, SetOpeningHoursDto input)
    {
        var restaurant = await _restaurantRepository.GetAsync(id);
        await CheckOwnershipAsync(restaurant);

        foreach (var hours in input.Hours)
        {
            restaurant.SetOpeningHours(hours.Day, hours.OpenTime, hours.CloseTime, hours.IsClosed);
        }

        await _restaurantRepository.UpdateAsync(restaurant);
    }

    #endregion

    #region Private Methods

    private async Task CheckOwnershipAsync(Restaurant restaurant)
    {
        if (restaurant.OwnerId != CurrentUser.Id)
        {
            // Check if admin
            if (!await AuthorizationService.IsGrantedAsync(RestaurantSvcPermissions.Admin.Default))
            {
                throw new BusinessException("Restaurant:NotOwner")
                    .WithData("restaurantId", restaurant.Id);
            }
        }
    }

    private IQueryable<Restaurant> ApplySorting(IQueryable<Restaurant> query, GetRestaurantListInput input)
    {
        if (string.IsNullOrEmpty(input.Sorting))
        {
            return query.OrderByDescending(r => r.AverageRating);
        }

        return input.Sorting.ToLower() switch
        {
            "name" => query.OrderBy(r => r.Name),
            "name desc" => query.OrderByDescending(r => r.Name),
            "rating" => query.OrderBy(r => r.AverageRating),
            "rating desc" => query.OrderByDescending(r => r.AverageRating),
            "deliveryfee" => query.OrderBy(r => r.DeliveryFee),
            "deliveryfee desc" => query.OrderByDescending(r => r.DeliveryFee),
            _ => query.OrderByDescending(r => r.AverageRating)
        };
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
