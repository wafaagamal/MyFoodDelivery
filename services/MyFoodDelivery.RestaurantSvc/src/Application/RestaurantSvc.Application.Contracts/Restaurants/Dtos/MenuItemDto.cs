using System;
using System.Collections.Generic;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public record MenuItemDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    decimal? DiscountedPrice,
    string? ImageUrl,
    int PreparationTimeMinutes,
    bool IsVegetarian,
    bool IsVegan,
    bool IsGlutenFree,
    bool IsSpicy,
    List<string> Allergens,
    bool IsAvailable,
    bool IsFeatured);
