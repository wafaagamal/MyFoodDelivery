using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderingSvc.Application.Orders.Queries;
using OrderingSvc.Domain.Orders;

namespace OrderingSvc.Application.Orders.Handlers;

public class OrderQueryHandlers :
    IRequestHandler<GetCartQuery, CartDto?>,
    IRequestHandler<GetOrderByIdQuery, OrderDetailDto?>,
    IRequestHandler<GetCustomerOrdersQuery, PagedResultDto<OrderListDto>>,
    IRequestHandler<GetActiveOrdersQuery, List<OrderListDto>>,
    IRequestHandler<GetOrderTrackingQuery, OrderTrackingDto?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly Application.Services.ICartService _cartService;

    public OrderQueryHandlers(
        IOrderRepository orderRepository,
        Application.Services.ICartService cartService)
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
    }

    public async Task<CartDto?> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(request.CustomerId);
        
        if (cart == null) return null;

        var totals = await _cartService.CalculateTotalsAsync(request.CustomerId, null);

        return new CartDto(
            cart.CustomerId,
            cart.RestaurantId,
            string.Empty,
            cart.Items.Select(i => new CartItemDto(
                i.MenuItemId,
                i.Name,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity,
                i.SpecialInstructions)).ToList(),
            totals.Subtotal,
            totals.DeliveryFee,
            totals.ServiceFee,
            totals.Total,
            0,
            true);
    }

    public async Task<OrderDetailDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetWithItemsAsync(request.OrderId);
        
        if (order == null) return null;

        return MapToDetailDto(order);
    }

    public async Task<PagedResultDto<OrderListDto>> Handle(GetCustomerOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = await _orderRepository.GetQueryableAsync();
        
        query = query.Where(o => o.CustomerId == request.CustomerId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<OrderStatus>(request.Status, out var status))
            {
                query = query.Where(o => o.Status == status);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreationTime)
            .Skip(request.SkipCount)
            .Take(request.MaxResultCount)
            .ToListAsync(cancellationToken);

        var items = orders.Select(MapToListDto).ToList();

        return new PagedResultDto<OrderListDto>(items, totalCount);
    }

    public async Task<List<OrderListDto>> Handle(GetActiveOrdersQuery request, CancellationToken cancellationToken)
    {
        var activeStatuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.PaymentConfirmed,
            OrderStatus.Preparing,
            OrderStatus.ReadyForPickup,
            OrderStatus.AwaitingPickup,
            OrderStatus.InTransit
        };

        var orders = await _orderRepository.GetByCustomerIdAsync(request.CustomerId);
        
        var activeOrders = orders
            .Where(o => activeStatuses.Contains(o.Status))
            .OrderByDescending(o => o.CreationTime)
            .Select(MapToListDto)
            .ToList();

        return activeOrders;
    }

    public async Task<OrderTrackingDto?> Handle(GetOrderTrackingQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetWithItemsAsync(request.OrderId);
        
        if (order == null) return null;

        var timeline = BuildTimeline(order);

        int? etaMinutes = null;
        string? etaText = null;

        if (order.Status == OrderStatus.Preparing)
        {
            etaMinutes = order.EstimatedPreparationMinutes;
            etaText = $"Food will be ready in ~{etaMinutes} minutes";
        }
        else if (order.Status == OrderStatus.InTransit)
        {
            etaMinutes = order.EstimatedDeliveryMinutes;
            etaText = $"Arriving in ~{etaMinutes} minutes";
        }

        RiderTrackingDto? riderTracking = null;
        if (order.RiderId.HasValue && 
            (order.Status == OrderStatus.AwaitingPickup || order.Status == OrderStatus.InTransit))
        {
            riderTracking = new RiderTrackingDto(
                order.RiderId.Value,
                "Rider", // Name would be fetched from DeliverySvc
                "", // Phone would be fetched from DeliverySvc
                0, // Current location from Redis
                0,
                2.5,
                etaMinutes ?? 15,
                "Motorcycle");
        }

        return new OrderTrackingDto(
            order.Id,
            order.OrderNumber,
            order.Status.ToString(),
            timeline,
            null, // Restaurant location from RestaurantSvc
            new LocationDto(
                order.DeliveryAddress.Latitude,
                order.DeliveryAddress.Longitude,
                "Delivery Address"),
            riderTracking,
            etaMinutes,
            etaText);
    }

    private static OrderDetailDto MapToDetailDto(Order order)
    {
        return new OrderDetailDto(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            "", // Customer name from CustomerSvc
            "", // Customer phone from CustomerSvc
            order.RestaurantId,
            "", // Restaurant name from RestaurantSvc
            "", // Restaurant phone from RestaurantSvc
            order.Items.Select(i => new OrderItemDto(
                i.MenuItemId,
                i.Name,
                i.Quantity,
                i.UnitPrice,
                i.TotalPrice,
                i.SpecialInstructions)).ToList(),
            new DeliveryAddressDto(
                order.DeliveryAddress.Street,
                order.DeliveryAddress.BuildingNumber,
                order.DeliveryAddress.Floor,
                order.DeliveryAddress.Apartment,
                order.DeliveryAddress.City,
                "", // Landmark not in value object
                order.DeliveryAddress.Latitude,
                order.DeliveryAddress.Longitude),
            new PaymentInfoDto(
                "Pending", // Payment method
                "Pending", // Payment status
                null, // Transaction ID
                order.TotalAmount,
                null, // Cash collected
                null), // Change
            order.Status.ToString(),
            order.SpecialInstructions,
            order.SubTotal,
            order.DeliveryFee,
            order.ServiceFee,
            order.Discount,
            0m, // Tip
            order.TotalAmount,
            order.CreationTime,
            order.PaymentConfirmedAt,
            order.PreparationStartedAt,
            order.ReadyForPickupAt,
            order.PickedUpAt,
            order.DeliveredAt,
            order.EstimatedPreparationMinutes + order.EstimatedDeliveryMinutes,
            order.RiderId.HasValue ? new RiderInfoDto(
                order.RiderId.Value,
                "", // Name from DeliverySvc
                "", // Phone from DeliverySvc
                null,
                null,
                null,
                null,
                null) : null);
    }

    private static OrderListDto MapToListDto(Order order)
    {
        return new OrderListDto(
            order.Id,
            order.OrderNumber,
            "", // Restaurant name from RestaurantSvc
            null, // Restaurant logo
            order.Status.ToString(),
            order.TotalAmount,
            order.Items.Count,
            order.CreationTime,
            order.EstimatedPreparationMinutes + order.EstimatedDeliveryMinutes);
    }

    private static List<TrackingEventDto> BuildTimeline(Order order)
    {
        var events = new List<TrackingEventDto>
        {
            // Order Placed
            new TrackingEventDto(
                "Placed",
                "Order Placed",
                "Your order has been received",
                order.CreationTime,
                true,
                order.Status == OrderStatus.Pending),

            // Payment Confirmed
            new TrackingEventDto(
                "PaymentConfirmed",
                "Payment Confirmed",
                "Payment confirmed",
                order.PaymentConfirmedAt,
                order.PaymentConfirmedAt.HasValue,
                order.Status == OrderStatus.PaymentConfirmed),

            // Preparing
            new TrackingEventDto(
                "Preparing",
                "Preparing",
                "Restaurant is preparing your food",
                order.PreparationStartedAt,
                order.PreparationStartedAt.HasValue,
                order.Status == OrderStatus.Preparing),

            // Ready for Pickup
            new TrackingEventDto(
                "ReadyForPickup",
                "Ready for Pickup",
                "Food is ready, waiting for rider",
                order.ReadyForPickupAt,
                order.ReadyForPickupAt.HasValue,
                order.Status == OrderStatus.ReadyForPickup || order.Status == OrderStatus.AwaitingPickup),

            // Picked Up
            new TrackingEventDto(
                "PickedUp",
                "On the Way",
                "Rider picked up your order",
                order.PickedUpAt,
                order.PickedUpAt.HasValue,
                order.Status == OrderStatus.InTransit),

            // Delivered
            new TrackingEventDto(
                "Delivered",
                "Delivered",
                "Order has been delivered",
                order.DeliveredAt,
                order.DeliveredAt.HasValue,
                order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Completed)
        };

        return events;
    }
}
