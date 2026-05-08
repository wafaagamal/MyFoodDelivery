using System;
using System.Collections.Generic;
using MediatR;

namespace RestaurantSvc.Application.Restaurants.Commands;

#region Restaurant Registration & Management

public record RegisterRestaurantCommand(
    Guid OwnerId,
    string Name,
    string Description,
    string CuisineType,
    string PhoneNumber,
    string Email,
    string Street,
    string BuildingNumber,
    string City,
    string? District,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude,
    decimal MinimumOrderAmount,
    decimal DeliveryFee,
    int EstimatedDeliveryMinutes) : IRequest<Guid>;

public record UpdateRestaurantInfoCommand(
    Guid RestaurantId,
    string Name,
    string Description,
    string CuisineType,
    string PhoneNumber,
    string Email,
    decimal MinimumOrderAmount,
    decimal DeliveryFee,
    int EstimatedDeliveryMinutes) : IRequest;

public record UpdateRestaurantAddressCommand(
    Guid RestaurantId,
    string Street,
    string BuildingNumber,
    string City,
    string? District,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude) : IRequest;

public record OpenRestaurantCommand(Guid RestaurantId) : IRequest;

public record CloseRestaurantCommand(Guid RestaurantId) : IRequest;

public record PauseOrdersCommand(Guid RestaurantId, string Reason) : IRequest;

public record ResumeOrdersCommand(Guid RestaurantId) : IRequest;

#endregion

#region Menu Category Commands

public record AddCategoryCommand(
    Guid RestaurantId,
    string Name,
    string? Description,
    int DisplayOrder) : IRequest<Guid>;

public record UpdateCategoryCommand(
    Guid RestaurantId,
    Guid CategoryId,
    string Name,
    string? Description,
    int DisplayOrder) : IRequest;

public record RemoveCategoryCommand(Guid RestaurantId, Guid CategoryId) : IRequest;

#endregion

#region Menu Item Commands

public record AddMenuItemCommand(
    Guid RestaurantId,
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
    List<string>? Allergens = null) : IRequest<Guid>;

public record UpdateMenuItemCommand(
    Guid RestaurantId,
    Guid MenuItemId,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    int PreparationTimeMinutes,
    bool IsVegetarian,
    bool IsVegan,
    bool IsGlutenFree,
    bool IsSpicy,
    List<string>? Allergens) : IRequest;

public record SetMenuItemAvailabilityCommand(
    Guid RestaurantId,
    Guid MenuItemId,
    bool IsAvailable) : IRequest;

public record UpdateMenuItemStockCommand(
    Guid RestaurantId,
    Guid MenuItemId,
    int Quantity) : IRequest;

public record RemoveMenuItemCommand(
    Guid RestaurantId,
    Guid MenuItemId) : IRequest;

#endregion

#region Order Handling Commands

public record ConfirmOrderCommand(
    Guid RestaurantId,
    Guid OrderId,
    int EstimatedPreparationMinutes) : IRequest;

public record RejectOrderCommand(
    Guid RestaurantId,
    Guid OrderId,
    string Reason) : IRequest;

public record StartPreparingOrderCommand(
    Guid RestaurantId,
    Guid OrderId) : IRequest;

public record MarkOrderReadyCommand(
    Guid RestaurantId,
    Guid OrderId) : IRequest;

#endregion

#region Opening Hours

public record SetOpeningHoursCommand(
    Guid RestaurantId,
    List<OpeningHoursDto> Hours) : IRequest;

public record OpeningHoursDto(
    DayOfWeek Day,
    TimeSpan OpenTime,
    TimeSpan CloseTime,
    bool IsClosed);

#endregion
