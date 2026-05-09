using System.Collections.Generic;

namespace RestaurantSvc.HttpApi.Dtos;

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
