using System;

namespace RestaurantSvc.Domain.Restaurants.Events;

public abstract record RestaurantDomainEvent
{
    public Guid RestaurantId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record RestaurantCreatedDomainEvent(
    Guid RestaurantId,
    Guid OwnerId,
    string Name) : RestaurantDomainEvent;

public record RestaurantUpdatedDomainEvent(
    Guid RestaurantId) : RestaurantDomainEvent;

public record RestaurantApprovedDomainEvent(
    Guid RestaurantId) : RestaurantDomainEvent;

public record RestaurantSuspendedDomainEvent(
    Guid RestaurantId,
    string Reason) : RestaurantDomainEvent;

public record RestaurantOpenedDomainEvent(
    Guid RestaurantId) : RestaurantDomainEvent;

public record RestaurantClosedDomainEvent(
    Guid RestaurantId) : RestaurantDomainEvent;

public record RestaurantPausedOrdersDomainEvent(
    Guid RestaurantId,
    string Reason) : RestaurantDomainEvent;

public record MenuItemAddedDomainEvent(
    Guid RestaurantId,
    Guid MenuItemId,
    string Name,
    decimal Price) : RestaurantDomainEvent;

public record MenuItemUpdatedDomainEvent(
    Guid RestaurantId,
    Guid MenuItemId) : RestaurantDomainEvent;

public record MenuItemUnavailableDomainEvent(
    Guid RestaurantId,
    Guid MenuItemId,
    string Name) : RestaurantDomainEvent;
