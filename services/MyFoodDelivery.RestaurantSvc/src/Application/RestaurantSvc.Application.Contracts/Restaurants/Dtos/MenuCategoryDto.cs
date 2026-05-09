using System;
using System.Collections.Generic;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public record MenuCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    int DisplayOrder,
    List<MenuItemDto> Items);
