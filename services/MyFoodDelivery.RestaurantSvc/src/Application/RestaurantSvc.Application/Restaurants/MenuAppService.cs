using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RestaurantSvc.Application.Contracts.Permissions;
using RestaurantSvc.Application.Contracts.Restaurants;
using RestaurantSvc.Domain.Restaurants;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using MyFoodDelivery.Shared.Events;

namespace RestaurantSvc.Application.Restaurants;

/// <summary>
/// ABP Application Service for menu management.
/// </summary>
[Authorize]
public class MenuAppService : ApplicationService, IMenuAppService
{
    private readonly IRepository<Restaurant, Guid> _restaurantRepository;
    private readonly IDistributedEventBus _eventBus;

    public MenuAppService(
        IRepository<Restaurant, Guid> restaurantRepository,
        IDistributedEventBus eventBus)
    {
        _restaurantRepository = restaurantRepository;
        _eventBus = eventBus;
    }

    #region Menu

    [AllowAnonymous]
    public async Task<MenuDto> GetMenuAsync(Guid restaurantId)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);

        var categories = restaurant.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new MenuCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                DisplayOrder = c.DisplayOrder,
                Items = restaurant.MenuItems
                    .Where(m => m.CategoryId == c.Id && m.IsActive)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => ObjectMapper.Map<MenuItem, MenuItemDto>(m))
                    .ToList()
            })
            .ToList();

        return new MenuDto
        {
            RestaurantId = restaurant.Id,
            RestaurantName = restaurant.Name,
            Categories = categories
        };
    }

    [AllowAnonymous]
    public async Task<MenuItemDto> GetMenuItemAsync(Guid restaurantId, Guid menuItemId)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        var menuItem = restaurant.GetMenuItemOrThrow(menuItemId);
        return ObjectMapper.Map<MenuItem, MenuItemDto>(menuItem);
    }

    #endregion

    #region Categories

    [Authorize(RestaurantSvcPermissions.Restaurants.ManageMenu)]
    public async Task<MenuCategoryDto> CreateCategoryAsync(Guid restaurantId, CreateMenuCategoryDto input)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        await CheckOwnershipAsync(restaurant);

        var categoryId = restaurant.AddCategory(input.Name, input.Description, input.DisplayOrder);
        await _restaurantRepository.UpdateAsync(restaurant);

        var category = restaurant.Categories.First(c => c.Id == categoryId);
        return new MenuCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            Items = new List<MenuItemDto>()
        };
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.ManageMenu)]
    public async Task<MenuCategoryDto> UpdateCategoryAsync(Guid restaurantId, Guid categoryId, UpdateMenuCategoryDto input)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        await CheckOwnershipAsync(restaurant);

        restaurant.UpdateCategory(categoryId, input.Name, input.Description, input.DisplayOrder);
        await _restaurantRepository.UpdateAsync(restaurant);

        var category = restaurant.Categories.First(c => c.Id == categoryId);
        return new MenuCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            Items = restaurant.MenuItems
                .Where(m => m.CategoryId == categoryId && m.IsActive)
                .Select(m => ObjectMapper.Map<MenuItem, MenuItemDto>(m))
                .ToList()
        };
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.ManageMenu)]
    public async Task DeleteCategoryAsync(Guid restaurantId, Guid categoryId)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        await CheckOwnershipAsync(restaurant);

        restaurant.RemoveCategory(categoryId);
        await _restaurantRepository.UpdateAsync(restaurant);
    }

    #endregion

    #region Menu Items

    [Authorize(RestaurantSvcPermissions.Restaurants.ManageMenu)]
    public async Task<MenuItemDto> CreateMenuItemAsync(Guid restaurantId, CreateMenuItemDto input)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        await CheckOwnershipAsync(restaurant);

        var menuItemId = restaurant.AddMenuItem(
            input.CategoryId,
            input.Name,
            input.Description,
            input.Price,
            input.ImageUrl,
            input.PreparationTimeMinutes,
            input.IsVegetarian,
            input.IsVegan,
            input.IsGlutenFree,
            input.IsSpicy,
            input.Allergens);

        await _restaurantRepository.UpdateAsync(restaurant);

        var menuItem = restaurant.GetMenuItemOrThrow(menuItemId);
        return ObjectMapper.Map<MenuItem, MenuItemDto>(menuItem);
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.ManageMenu)]
    public async Task<MenuItemDto> UpdateMenuItemAsync(Guid restaurantId, Guid menuItemId, UpdateMenuItemDto input)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        await CheckOwnershipAsync(restaurant);

        restaurant.UpdateMenuItem(
            menuItemId,
            input.Name,
            input.Description,
            input.Price,
            input.ImageUrl,
            input.PreparationTimeMinutes,
            input.IsVegetarian,
            input.IsVegan,
            input.IsGlutenFree,
            input.IsSpicy,
            input.Allergens);

        await _restaurantRepository.UpdateAsync(restaurant);

        var menuItem = restaurant.GetMenuItemOrThrow(menuItemId);
        return ObjectMapper.Map<MenuItem, MenuItemDto>(menuItem);
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.ManageMenu)]
    public async Task SetMenuItemAvailabilityAsync(Guid restaurantId, Guid menuItemId, SetMenuItemAvailabilityDto input)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        await CheckOwnershipAsync(restaurant);

        restaurant.SetMenuItemAvailability(menuItemId, input.IsAvailable);
        await _restaurantRepository.UpdateAsync(restaurant);

        if (!input.IsAvailable)
        {
            var menuItem = restaurant.GetMenuItemOrThrow(menuItemId);
            await _eventBus.PublishAsync(new MenuItemUnavailableEto(
                restaurant.Id,
                menuItemId,
                menuItem.Name));
        }
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.ManageMenu)]
    public async Task UpdateMenuItemStockAsync(Guid restaurantId, Guid menuItemId, UpdateMenuItemStockDto input)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        await CheckOwnershipAsync(restaurant);

        restaurant.UpdateMenuItemStock(menuItemId, input.Quantity);
        await _restaurantRepository.UpdateAsync(restaurant);
    }

    [Authorize(RestaurantSvcPermissions.Restaurants.ManageMenu)]
    public async Task DeleteMenuItemAsync(Guid restaurantId, Guid menuItemId)
    {
        var restaurant = await _restaurantRepository.GetAsync(restaurantId);
        await CheckOwnershipAsync(restaurant);

        restaurant.RemoveMenuItem(menuItemId);
        await _restaurantRepository.UpdateAsync(restaurant);
    }

    #endregion

    #region Private Methods

    private async Task CheckOwnershipAsync(Restaurant restaurant)
    {
        if (restaurant.OwnerId != CurrentUser.Id)
        {
            if (!await AuthorizationService.IsGrantedAsync(RestaurantSvcPermissions.Admin.Default))
            {
                throw new BusinessException("Restaurant:NotOwner")
                    .WithData("restaurantId", restaurant.Id);
            }
        }
    }

    #endregion
}
