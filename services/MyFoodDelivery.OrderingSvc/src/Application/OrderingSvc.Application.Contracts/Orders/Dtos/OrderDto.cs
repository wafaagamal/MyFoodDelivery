using System;
using System.Collections.Generic;
using OrderingSvc.Domain.Orders;
using Volo.Abp.Application.Dtos;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

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
    public Guid? PaymentMethodId { get; set; }
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

