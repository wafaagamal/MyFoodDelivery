using System;
using System.Collections.Generic;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public record MenuDto(
    Guid RestaurantId,
    string RestaurantName,
    List<MenuCategoryDto> Categories);
