using System;

namespace MyFoodDelivery.Shared.Events;

/// <summary>
/// Published frequently by riders to update their real-time location.
/// Consumed by DeliverySvc for Redis Geo storage and SignalR broadcast.
/// </summary>
public record RiderLocationUpdatedEto(
    Guid RiderId,
    double Latitude,
    double Longitude,
    double? Heading,
    double? Speed,
    DateTime Timestamp);

/// <summary>
/// Published when a rider is assigned to an order.
/// Consumed by OrderingSvc, CustomerSvc for notifications.
/// </summary>
public record RiderAssignedEto(
    Guid OrderId,
    Guid RiderId,
    string RiderName,
    string RiderPhoneNumber,
    double EstimatedPickupMinutes,
    double EstimatedDeliveryMinutes,
    DateTime AssignedAt);

/// <summary>
/// Published when a rider goes online/available for orders.
/// </summary>
public record RiderAvailableEto(
    Guid RiderId,
    double Latitude,
    double Longitude,
    DateTime AvailableAt);

/// <summary>
/// Published when a rider goes offline.
/// </summary>
public record RiderOfflineEto(
    Guid RiderId,
    DateTime OfflineAt,
    RiderOfflineReason Reason);

/// <summary>
/// Published when a rider accepts an order assignment.
/// </summary>
public record RiderAcceptedOrderEto(
    Guid OrderId,
    Guid RiderId,
    DateTime AcceptedAt,
    int EstimatedPickupMinutes);

/// <summary>
/// Published when a rider declines an order assignment.
/// Triggers reassignment in DeliverySvc.
/// </summary>
public record RiderDeclinedOrderEto(
    Guid OrderId,
    Guid RiderId,
    DateTime DeclinedAt,
    string? Reason);

/// <summary>
/// Published when a rider arrives at the restaurant.
/// </summary>
public record RiderArrivedAtRestaurantEto(
    Guid OrderId,
    Guid RiderId,
    Guid RestaurantId,
    DateTime ArrivedAt);

/// <summary>
/// Published when a rider arrives at the delivery location.
/// </summary>
public record RiderArrivedAtDeliveryEto(
    Guid OrderId,
    Guid RiderId,
    Guid CustomerId,
    DateTime ArrivedAt);

/// <summary>
/// Published when estimated delivery time changes significantly.
/// </summary>
public record DeliveryEtaUpdatedEto(
    Guid OrderId,
    Guid RiderId,
    DateTime PreviousEta,
    DateTime NewEta,
    EtaChangeReason Reason);

/// <summary>
/// Published for delivery-related issues.
/// </summary>
public record DeliveryIssueReportedEto(
    Guid OrderId,
    Guid RiderId,
    DeliveryIssueType IssueType,
    string Description,
    DateTime ReportedAt);

public enum RiderOfflineReason
{
    EndOfShift,
    Break,
    VehicleIssue,
    Emergency,
    AppClosed,
    Unknown
}

public enum EtaChangeReason
{
    TrafficDelay,
    WeatherDelay,
    RestaurantDelay,
    RiderChanged,
    RouteChange,
    Other
}

public enum DeliveryIssueType
{
    CannotFindAddress,
    CustomerUnreachable,
    VehicleBreakdown,
    AccidentOrSafety,
    OrderDamaged,
    WrongAddress,
    Other
}
