using System;
using System.Collections.Generic;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public record RestaurantOrderItemDto(
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string? SpecialInstructions);

public record RestaurantOrderDto(
    Guid OrderId,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    List<RestaurantOrderItemDto> Items,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? ReadyAt,
    string? SpecialInstructions);
