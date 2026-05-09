using System;
using System.Collections.Generic;
using OrderingSvc.Domain.Orders;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

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

