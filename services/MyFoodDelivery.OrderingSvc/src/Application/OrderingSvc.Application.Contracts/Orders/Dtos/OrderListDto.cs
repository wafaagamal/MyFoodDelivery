using System;
using OrderingSvc.Domain.Orders;
using Volo.Abp.Application.Dtos;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

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

