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
[Route("api/restaurants")]
public class RestaurantController : AbpController
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public RestaurantController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    #region Public Endpoints

    /// <summary>
    /// Search restaurants by name, cuisine, location
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<RestaurantPagedResult<RestaurantListDto>>> Search(
        [FromQuery] string? search,
        [FromQuery] string? cuisine,
        [FromQuery] double? lat,
        [FromQuery] double? lng,
        [FromQuery] double? radius,
        [FromQuery] bool? openNow,
        [FromQuery] decimal? maxFee,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20)
    {
        var result = await _mediator.Send(new SearchRestaurantsQuery(
            search, cuisine, lat, lng, radius, openNow, maxFee, skip, take));
        return Ok(result);
    }

    /// <summary>
    /// Get restaurant details
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<RestaurantDetailDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRestaurantByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get restaurant menu
    /// </summary>
    [HttpGet("{id:guid}/menu")]
    [AllowAnonymous]
    public async Task<ActionResult<MenuDto>> GetMenu(Guid id)
    {
        var result = await _mediator.Send(new GetMenuQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Get nearby restaurants
    /// </summary>
    [HttpGet("nearby")]
    [AllowAnonymous]
    public async Task<ActionResult<List<NearbyRestaurantDto>>> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius = 5,
        [FromQuery] int maxResults = 20)
    {
        var result = await _mediator.Send(new GetNearbyRestaurantsQuery(lat, lng, radius, maxResults));
        return Ok(result);
    }

    #endregion

    #region Restaurant Owner Endpoints

    /// <summary>
    /// Register a new restaurant
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Guid>> Register([FromBody] RegisterRestaurantRequest request)
    {
        var restaurantId = await _mediator.Send(new RegisterRestaurantCommand(
            _currentUser.GetId(),
            request.Name,
            request.Description,
            request.CuisineType,
            request.PhoneNumber,
            request.Email,
            request.Street,
            request.BuildingNumber,
            request.City,
            request.District,
            request.PostalCode,
            request.Country,
            request.Latitude,
            request.Longitude,
            request.MinimumOrderAmount,
            request.DeliveryFee,
            request.EstimatedDeliveryMinutes));

        return CreatedAtAction(nameof(GetById), new { id = restaurantId }, restaurantId);
    }

    /// <summary>
    /// Get my restaurants
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<List<RestaurantListDto>>> GetMyRestaurants()
    {
        var result = await _mediator.Send(new GetRestaurantsByOwnerQuery(_currentUser.GetId()));
        return Ok(result);
    }

    /// <summary>
    /// Update restaurant info
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRestaurantRequest request)
    {
        await _mediator.Send(new UpdateRestaurantInfoCommand(
            id,
            request.Name,
            request.Description,
            request.CuisineType,
            request.PhoneNumber,
            request.Email,
            request.MinimumOrderAmount,
            request.DeliveryFee,
            request.EstimatedDeliveryMinutes));

        return NoContent();
    }

    /// <summary>
    /// Open restaurant
    /// </summary>
    [HttpPost("{id:guid}/open")]
    [Authorize]
    public async Task<IActionResult> Open(Guid id)
    {
        await _mediator.Send(new OpenRestaurantCommand(id));
        return NoContent();
    }

    /// <summary>
    /// Close restaurant
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [Authorize]
    public async Task<IActionResult> Close(Guid id)
    {
        await _mediator.Send(new CloseRestaurantCommand(id));
        return NoContent();
    }

    /// <summary>
    /// Pause accepting orders
    /// </summary>
    [HttpPost("{id:guid}/pause-orders")]
    [Authorize]
    public async Task<IActionResult> PauseOrders(Guid id, [FromBody] PauseOrdersRequest request)
    {
        await _mediator.Send(new PauseOrdersCommand(id, request.Reason));
        return NoContent();
    }

    #endregion
}
