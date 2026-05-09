using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantSvc.Application.Contracts.Restaurants.Dtos;
using RestaurantSvc.Application.Restaurants.Commands;
using RestaurantSvc.Application.Restaurants.Queries;
using RestaurantSvc.HttpApi.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Users;

namespace RestaurantSvc.HttpApi.Controllers;

[ApiController]
[Route("api/restaurants/{restaurantId:guid}/menu")]
[Authorize]
public class MenuController : AbpController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public MenuController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    #region Categories

    /// <summary>
    /// Add a menu category
    /// </summary>
    [HttpPost("categories")]
    public async Task<ActionResult<Guid>> AddCategory(
        Guid restaurantId,
        [FromBody] AddCategoryRequest request)
    {
        var categoryId = await _mediator.Send(new AddCategoryCommand(
            restaurantId,
            request.Name,
            request.Description,
            request.DisplayOrder));

        return CreatedAtAction(nameof(GetMenuItem), new { restaurantId, menuItemId = categoryId }, categoryId);
    }

    /// <summary>
    /// Update a menu category
    /// </summary>
    [HttpPut("categories/{categoryId:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid restaurantId,
        Guid categoryId,
        [FromBody] UpdateCategoryRequest request)
    {
        await _mediator.Send(new UpdateCategoryCommand(
            restaurantId,
            categoryId,
            request.Name,
            request.Description,
            request.DisplayOrder));

        return NoContent();
    }

    /// <summary>
    /// Remove a menu category
    /// </summary>
    [HttpDelete("categories/{categoryId:guid}")]
    public async Task<IActionResult> RemoveCategory(Guid restaurantId, Guid categoryId)
    {
        await _mediator.Send(new RemoveCategoryCommand(restaurantId, categoryId));
        return NoContent();
    }

    #endregion

    #region Menu Items

    /// <summary>
    /// Add a menu item
    /// </summary>
    [HttpPost("items")]
    public async Task<ActionResult<Guid>> AddMenuItem(
        Guid restaurantId,
        [FromBody] AddMenuItemRequest request)
    {
        var menuItemId = await _mediator.Send(new AddMenuItemCommand(
            restaurantId,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.PreparationTimeMinutes,
            request.IsVegetarian,
            request.IsVegan,
            request.IsGlutenFree,
            request.IsSpicy,
            request.Allergens));

        return CreatedAtAction(nameof(GetMenuItem), new { restaurantId, menuItemId }, menuItemId);
    }

    /// <summary>
    /// Get menu item details
    /// </summary>
    [HttpGet("items/{menuItemId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<MenuItemDto>> GetMenuItem(Guid restaurantId, Guid menuItemId)
    {
        var result = await _mediator.Send(new GetMenuItemQuery(restaurantId, menuItemId));
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Update a menu item
    /// </summary>
    [HttpPut("items/{menuItemId:guid}")]
    public async Task<IActionResult> UpdateMenuItem(
        Guid restaurantId,
        Guid menuItemId,
        [FromBody] UpdateMenuItemRequest request)
    {
        await _mediator.Send(new UpdateMenuItemCommand(
            restaurantId,
            menuItemId,
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.PreparationTimeMinutes,
            request.IsVegetarian,
            request.IsVegan,
            request.IsGlutenFree,
            request.IsSpicy,
            request.Allergens));

        return NoContent();
    }

    /// <summary>
    /// Set menu item availability
    /// </summary>
    [HttpPatch("items/{menuItemId:guid}/availability")]
    public async Task<IActionResult> SetAvailability(
        Guid restaurantId,
        Guid menuItemId,
        [FromBody] SetAvailabilityRequest request)
    {
        await _mediator.Send(new SetMenuItemAvailabilityCommand(
            restaurantId,
            menuItemId,
            request.IsAvailable));

        return NoContent();
    }

    /// <summary>
    /// Update menu item stock
    /// </summary>
    [HttpPatch("items/{menuItemId:guid}/stock")]
    public async Task<IActionResult> UpdateStock(
        Guid restaurantId,
        Guid menuItemId,
        [FromBody] UpdateStockRequest request)
    {
        await _mediator.Send(new UpdateMenuItemStockCommand(
            restaurantId,
            menuItemId,
            request.Quantity));

        return NoContent();
    }

    /// <summary>
    /// Remove a menu item
    /// </summary>
    [HttpDelete("items/{menuItemId:guid}")]
    public async Task<IActionResult> RemoveMenuItem(Guid restaurantId, Guid menuItemId)
    {
        await _mediator.Send(new RemoveMenuItemCommand(restaurantId, menuItemId));
        return NoContent();
    }

    #endregion
}

#region Request Models

public record AddCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder);

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder);

public record AddMenuItemRequest(
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    int PreparationTimeMinutes,
    bool IsVegetarian = false,
    bool IsVegan = false,
    bool IsGlutenFree = false,
    bool IsSpicy = false,
    List<string>? Allergens = null);

public record UpdateMenuItemRequest(
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    int PreparationTimeMinutes,
    bool IsVegetarian,
    bool IsVegan,
    bool IsGlutenFree,
    bool IsSpicy,
    List<string>? Allergens);

public record SetAvailabilityRequest(bool IsAvailable);

public record UpdateStockRequest(int Quantity);

#endregion
