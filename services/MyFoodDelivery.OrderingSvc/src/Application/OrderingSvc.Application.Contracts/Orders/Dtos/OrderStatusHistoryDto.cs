using System;
using OrderingSvc.Domain.Orders;

namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class OrderStatusHistoryDto
{
    public OrderStatus Status { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Note { get; set; }
}

