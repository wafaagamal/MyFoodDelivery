using System;
using OrderCancellationReason = MyFoodDelivery.Shared.Events.OrderCancellationReason;
using OrderStatus = OrderingSvc.Domain.Orders.OrderStatus;

namespace OrderingSvc.Domain.Orders.Events;

/// <summary>
/// Base class for Order domain events.
/// </summary>
public abstract record OrderDomainEvent
{
    public Guid OrderId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

public record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId) : OrderDomainEvent;

public record OrderPaymentConfirmedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    decimal TotalAmount) : OrderDomainEvent;

public record OrderPreparationStartedDomainEvent(
    Guid OrderId,
    Guid RestaurantId) : OrderDomainEvent;

public record OrderReadyForPickupDomainEvent(
    Guid OrderId,
    Guid RestaurantId) : OrderDomainEvent;

public record RiderAssignedDomainEvent(
    Guid OrderId,
    Guid RiderId,
    string RiderName) : OrderDomainEvent;

public record OrderPickedUpDomainEvent(
    Guid OrderId,
    Guid RiderId) : OrderDomainEvent;

public record OrderDeliveredDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RiderId,
    int LoyaltyPointsEarned) : OrderDomainEvent;

public record OrderCompletedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    Guid RiderId,
    decimal TotalAmount) : OrderDomainEvent;

public record OrderCancelledDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid? RiderId,
    OrderStatus PreviousStatus,
    OrderCancellationReason Reason,
    string? Details) : OrderDomainEvent;

public record DeliveryEtaUpdatedDomainEvent(
    Guid OrderId,
    DateTime NewEta) : OrderDomainEvent;
