using System;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public record OpeningHoursDto(
    DayOfWeek Day,
    TimeSpan OpenTime,
    TimeSpan CloseTime,
    bool IsClosed);
