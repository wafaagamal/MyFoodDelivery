using System;
using System.Collections.Generic;

namespace MyFoodDelivery.Shared.Events;

/// <summary>
/// Published when a new order is created and pending payment.
/// Consumed by DeliverySvc (for rider pre-assignment), RestaurantSvc (for stock reservation).
/// </summary>
public record OrderCreatedEto(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    string DeliveryStreet,
    string DeliveryCity,
    string DeliveryPostalCode,
    double DeliveryLatitude,
    double DeliveryLongitude,
    decimal TotalAmount,
    decimal DeliveryFee,
    List<OrderItemEto> Items,
    DateTime CreatedAt);

public record OrderItemEto(
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string? SpecialInstructions);

/// <summary>
/// Published when payment for an order is confirmed.
/// Triggers order preparation at restaurant and delivery assignment.
/// </summary>
public record OrderPaymentConfirmedEto(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    DateTime ConfirmedAt);

/// <summary>
/// Published when the restaurant marks the order as ready for pickup.
/// Triggers rider dispatch notification.
/// </summary>
public record OrderReadyForPickupEto(
    Guid OrderId,
    Guid RestaurantId,
    DateTime ReadyAt);

/// <summary>
/// Published when order status changes.
/// Consumed by CustomerSvc for notifications, analytics.
/// </summary>
public record OrderStatusChangedEto(
    Guid OrderId,
    Guid CustomerId,
    OrderStatus PreviousStatus,
    OrderStatus NewStatus,
    DateTime ChangedAt,
    string? Reason = null);

/// <summary>
/// Published when an order is completed successfully.
/// Triggers loyalty points award in CustomerSvc.
/// </summary>
public record OrderCompletedEto(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    Guid RiderId,
    decimal TotalAmount,
    int LoyaltyPointsEarned,
    DateTime CompletedAt);

/// <summary>
/// Published when an order is cancelled.
/// Triggers refund processing, stock restoration, rider release.
/// </summary>
public record OrderCancelledEto(
    Guid OrderId,
    Guid CustomerId,
    Guid? RiderId,
    OrderCancellationReason Reason,
    string? Details,
    DateTime CancelledAt);

/// <summary>
/// Published when a rider picks up the order from the restaurant.
/// </summary>
public record OrderPickedUpEto(
    Guid OrderId,
    Guid RiderId,
    DateTime PickedUpAt);

/// <summary>
/// Published when an order is delivered to the customer.
/// </summary>
public record OrderDeliveredEto(
    Guid OrderId,
    Guid RiderId,
    Guid CustomerId,
    DateTime DeliveredAt);

public enum OrderStatus
{
    Pending,
    PaymentConfirmed,
    Preparing,
    ReadyForPickup,
    AwaitingRider,
    RiderAssigned,
    PickedUp,
    InTransit,
    Delivered,
    Completed,
    Cancelled
}

public enum OrderCancellationReason
{
    CustomerRequested,
    RestaurantUnavailable,
    PaymentFailed,
    NoRiderAvailable,
    DeliveryTimeout,
    FraudSuspected,
    Other
}
