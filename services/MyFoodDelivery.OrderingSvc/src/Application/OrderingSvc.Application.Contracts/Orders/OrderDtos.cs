using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace OrderingSvc.Application.Contracts.Orders;

#region Order DTOs

public class OrderDto : FullAuditedEntityDto<Guid>
{
    public string OrderNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public string CustomerPhone { get; set; } = default!;
    public Guid RestaurantId { get; set; }
    public string RestaurantName { get; set; } = default!;
    public List<OrderItemDto> Items { get; set; } = new();
    public DeliveryAddressDto DeliveryAddress { get; set; } = default!;
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal Tip { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string? PromoCode { get; set; }
    public string? Notes { get; set; }
    public Guid? RiderId { get; set; }
    public string? RiderName { get; set; }
    public string? RiderPhone { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }
    public int? Rating { get; set; }
    public string? Review { get; set; }
}

public class OrderListDto : EntityDto<Guid>
{
    public string OrderNumber { get; set; } = default!;
    public string RestaurantName { get; set; } = default!;
    public string? RestaurantLogoUrl { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
}

public class OrderItemDto
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SpecialInstructions { get; set; }
}

public class DeliveryAddressDto
{
    public string Street { get; set; } = default!;
    public string BuildingNumber { get; set; } = default!;
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public string? Landmark { get; set; }
    public string? DeliveryInstructions { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public enum OrderStatus
{
    Pending = 0,
    PaymentConfirmed = 1,
    Preparing = 2,
    ReadyForPickup = 3,
    AwaitingPickup = 4,
    InTransit = 5,
    Delivered = 6,
    Completed = 7,
    Cancelled = 8
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    Wallet = 2
}

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}

#endregion

#region Order Tracking DTOs

public class OrderTrackingDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = default!;
    public OrderStatus Status { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    public RestaurantLocationDto Restaurant { get; set; } = default!;
    public DeliveryAddressDto DeliveryAddress { get; set; } = default!;
    public RiderTrackingDto? Rider { get; set; }
    public List<OrderStatusHistoryDto> StatusHistory { get; set; } = new();
}

public class RestaurantLocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class RiderTrackingDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? PhotoUrl { get; set; }
    public double? CurrentLatitude { get; set; }
    public double? CurrentLongitude { get; set; }
    public int? EtaMinutes { get; set; }
}

public class OrderStatusHistoryDto
{
    public OrderStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Note { get; set; }
}

#endregion

#region Create/Update DTOs

public class CreateOrderDto
{
    [Required]
    public DeliveryAddressDto DeliveryAddress { get; set; } = default!;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Payment method ID for card payments (Stripe PaymentMethod)
    /// </summary>
    public string? PaymentMethodId { get; set; }
}

public class CancelOrderDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = default!;
}

public class RateOrderDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Review { get; set; }

    [Range(1, 5)]
    public int? DeliveryRating { get; set; }

    [StringLength(1000)]
    public string? DeliveryReview { get; set; }
}

#endregion

#region Input DTOs

public class GetOrderListInput : PagedAndSortedResultRequestDto
{
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetRestaurantOrdersInput : PagedAndSortedResultRequestDto
{
    public Guid RestaurantId { get; set; }
    public OrderStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

#endregion
