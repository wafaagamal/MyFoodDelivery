using System;
using System.Collections.Generic;

namespace MyFoodDelivery.Shared.Events;

/// <summary>
/// Published when a new restaurant is registered and approved.
/// </summary>
public record RestaurantRegisteredEto(
    Guid RestaurantId,
    Guid OwnerId,
    string Name,
    string CuisineType,
    double Latitude,
    double Longitude);

/// <summary>
/// Published when a restaurant opens/closes.
/// </summary>
public record RestaurantStatusChangedEto(
    Guid RestaurantId,
    bool IsOpen,
    bool AcceptingOrders);

/// <summary>
/// Published when a menu item becomes unavailable.
/// </summary>
public record MenuItemUnavailableEto(
    Guid RestaurantId,
    Guid MenuItemId,
    string MenuItemName);

/// <summary>
/// Published when restaurant confirms it received an order.
/// </summary>
public record RestaurantConfirmedOrderEto(
    Guid OrderId,
    Guid RestaurantId,
    int EstimatedPreparationMinutes);

/// <summary>
/// Published when restaurant starts preparing an order.
/// </summary>
public record RestaurantStartedPreparingEto(
    Guid OrderId,
    Guid RestaurantId,
    DateTime StartedAt);

/// <summary>
/// Published when order is ready for pickup.
/// </summary>
public record RestaurantOrderReadyEto(
    Guid OrderId,
    Guid RestaurantId,
    DateTime ReadyAt);

/// <summary>
/// Published when restaurant rejects an order.
/// </summary>
public record RestaurantRejectedOrderEto(
    Guid OrderId,
    Guid RestaurantId,
    string Reason);
