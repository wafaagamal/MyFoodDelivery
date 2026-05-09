using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantSvc.Application.Contracts.Restaurants.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace RestaurantSvc.Application.Contracts.Restaurants;

/// <summary>
/// Application service interface for restaurant management.
/// Follows ABP naming conventions.
/// </summary>
public interface IRestaurantAppService : IApplicationService
{
    #region Restaurant CRUD

    /// <summary>
    /// Get restaurant by ID
    /// </summary>
    Task<RestaurantDto> GetAsync(Guid id);

    /// <summary>
    /// Get paginated list of restaurants with filtering
    /// </summary>
    Task<PagedResultDto<RestaurantListDto>> GetListAsync(GetRestaurantListInput input);

    /// <summary>
    /// Get restaurants by owner
    /// </summary>
    Task<List<RestaurantListDto>> GetByOwnerAsync();

    /// <summary>
    /// Get nearby restaurants
    /// </summary>
    Task<List<NearbyRestaurantDto>> GetNearbyAsync(GetNearbyRestaurantsInput input);

    /// <summary>
    /// Create a new restaurant
    /// </summary>
    Task<RestaurantDto> CreateAsync(CreateRestaurantDto input);

    /// <summary>
    /// Update restaurant information
    /// </summary>
    Task<RestaurantDto> UpdateAsync(Guid id, UpdateRestaurantDto input);

    /// <summary>
    /// Delete a restaurant
    /// </summary>
    Task DeleteAsync(Guid id);

    #endregion

    #region Restaurant Status

    /// <summary>
    /// Open the restaurant
    /// </summary>
    Task OpenAsync(Guid id);

    /// <summary>
    /// Close the restaurant
    /// </summary>
    Task CloseAsync(Guid id);

    /// <summary>
    /// Pause accepting orders
    /// </summary>
    Task PauseOrdersAsync(Guid id, string reason);

    /// <summary>
    /// Resume accepting orders
    /// </summary>
    Task ResumeOrdersAsync(Guid id);

    /// <summary>
    /// Set opening hours
    /// </summary>
    Task SetOpeningHoursAsync(Guid id, SetOpeningHoursDto input);

    #endregion
}

/// <summary>
/// Application service interface for menu management.
/// </summary>
public interface IMenuAppService : IApplicationService
{
    #region Menu

    /// <summary>
    /// Get full menu for a restaurant
    /// </summary>
    Task<MenuDto> GetMenuAsync(Guid restaurantId);

    /// <summary>
    /// Get a specific menu item
    /// </summary>
    Task<MenuItemDto> GetMenuItemAsync(Guid restaurantId, Guid menuItemId);

    #endregion

    #region Categories

    /// <summary>
    /// Add a menu category
    /// </summary>
    Task<MenuCategoryDto> CreateCategoryAsync(Guid restaurantId, CreateMenuCategoryDto input);

    /// <summary>
    /// Update a menu category
    /// </summary>
    Task<MenuCategoryDto> UpdateCategoryAsync(Guid restaurantId, Guid categoryId, UpdateMenuCategoryDto input);

    /// <summary>
    /// Remove a menu category
    /// </summary>
    Task DeleteCategoryAsync(Guid restaurantId, Guid categoryId);

    #endregion

    #region Menu Items

    /// <summary>
    /// Add a menu item
    /// </summary>
    Task<MenuItemDto> CreateMenuItemAsync(Guid restaurantId, CreateMenuItemDto input);

    /// <summary>
    /// Update a menu item
    /// </summary>
    Task<MenuItemDto> UpdateMenuItemAsync(Guid restaurantId, Guid menuItemId, UpdateMenuItemDto input);

    /// <summary>
    /// Set menu item availability
    /// </summary>
    Task SetMenuItemAvailabilityAsync(Guid restaurantId, Guid menuItemId, SetMenuItemAvailabilityDto input);

    /// <summary>
    /// Update menu item stock
    /// </summary>
    Task UpdateMenuItemStockAsync(Guid restaurantId, Guid menuItemId, UpdateMenuItemStockDto input);

    /// <summary>
    /// Delete a menu item
    /// </summary>
    Task DeleteMenuItemAsync(Guid restaurantId, Guid menuItemId);

    #endregion
}
