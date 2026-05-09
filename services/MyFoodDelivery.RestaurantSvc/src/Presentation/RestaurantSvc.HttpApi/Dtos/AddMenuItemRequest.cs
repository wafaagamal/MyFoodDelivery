using System;
using System.Collections.Generic;

namespace RestaurantSvc.HttpApi.Dtos;

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
