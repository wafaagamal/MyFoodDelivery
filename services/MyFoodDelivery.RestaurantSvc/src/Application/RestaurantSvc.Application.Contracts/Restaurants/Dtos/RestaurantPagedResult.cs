using System.Collections.Generic;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

/// <summary>
/// Generic paged result wrapper used by restaurant query handlers.
/// </summary>
public record RestaurantPagedResult<T>(
    List<T> Items,
    int TotalCount);
