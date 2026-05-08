using System;
using System.Collections.Generic;
using MediatR;

namespace OrderingSvc.Application.Orders.Queries;

#region Cart Queries

public record GetCartQuery(Guid CustomerId) : IRequest<CartDto?>;

public record GetCartTotalQuery(Guid CustomerId) : IRequest<CartTotalDto>;

#endregion

#region Order Queries

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDetailDto?>;

public record GetOrderByNumberQuery(string OrderNumber) : IRequest<OrderDetailDto?>;

public record GetCustomerOrdersQuery(
    Guid CustomerId,
    string? Status,
    int SkipCount = 0,
    int MaxResultCount = 20) : IRequest<PagedResultDto<OrderListDto>>;

public record GetActiveOrdersQuery(Guid CustomerId) : IRequest<List<OrderListDto>>;

public record GetOrderTrackingQuery(Guid OrderId) : IRequest<OrderTrackingDto?>;

#endregion

#region DTOs

public record CartDto(
    Guid CustomerId,
    Guid RestaurantId,
    string RestaurantName,
    List<CartItemDto> Items,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal ServiceFee,
    decimal Total,
    decimal MinimumOrderAmount,
    bool MeetsMinimum);

public record CartItemDto(
    Guid MenuItemId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    string? SpecialInstructions);

public record CartTotalDto(
    decimal Subtotal,
    decimal DeliveryFee,
    decimal ServiceFee,
    decimal Discount,
    decimal Total);

public record OrderDetailDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    Guid RestaurantId,
    string RestaurantName,
    string RestaurantPhone,
    List<OrderItemDto> Items,
    DeliveryAddressDto DeliveryAddress,
    PaymentInfoDto Payment,
    string Status,
    string? SpecialInstructions,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal ServiceFee,
    decimal Discount,
    decimal Tip,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? PreparingAt,
    DateTime? ReadyAt,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt,
    int? EstimatedMinutes,
    RiderInfoDto? Rider);

public record OrderListDto(
    Guid Id,
    string OrderNumber,
    string RestaurantName,
    string? RestaurantLogo,
    string Status,
    decimal TotalAmount,
    int ItemCount,
    DateTime CreatedAt,
    int? EstimatedMinutes);

public record OrderItemDto(
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? SpecialInstructions);

public record DeliveryAddressDto(
    string Street,
    string BuildingNumber,
    string? Floor,
    string? Apartment,
    string City,
    string? Landmark,
    double Latitude,
    double Longitude);

public record PaymentInfoDto(
    string Method,
    string Status,
    string? TransactionId,
    decimal Amount,
    decimal? CashCollected,
    decimal? ChangeAmount);

public record RiderInfoDto(
    Guid RiderId,
    string Name,
    string Phone,
    string? PhotoUrl,
    double? CurrentLatitude,
    double? CurrentLongitude,
    double? DistanceKm,
    int? EtaMinutes);

public record OrderTrackingDto(
    Guid OrderId,
    string OrderNumber,
    string Status,
    List<TrackingEventDto> Timeline,
    LocationDto? RestaurantLocation,
    LocationDto? DeliveryLocation,
    RiderTrackingDto? Rider,
    int? EtaMinutes,
    string? EtaText);

public record TrackingEventDto(
    string Status,
    string Title,
    string? Description,
    DateTime? OccurredAt,
    bool IsCompleted,
    bool IsCurrent);

public record LocationDto(
    double Latitude,
    double Longitude,
    string? Label);

public record RiderTrackingDto(
    Guid RiderId,
    string Name,
    string Phone,
    double Latitude,
    double Longitude,
    double DistanceKm,
    int EtaMinutes,
    string VehicleType);

public record PagedResultDto<T>(
    List<T> Items,
    int TotalCount);

#endregion
