using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrderingSvc.Application.Contracts.Orders;
using OrderingSvc.Application.Contracts.Orders.Dtos;
using OrderingSvc.Application.Contracts.Permissions;
using OrderingSvc.Domain.Orders;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Http.Modeling;
using MyFoodDelivery.Shared.Events;
using DomainOrderStatus = OrderingSvc.Domain.Orders.OrderStatus;
using EventOrderStatus = MyFoodDelivery.Shared.Events.OrderStatus;
using OrderCancellationReason = MyFoodDelivery.Shared.Events.OrderCancellationReason;

namespace OrderingSvc.Application.Orders;

/// <summary>
/// ABP Application Service for restaurant order management.
/// Auto API generation disabled - use RestaurantOrderController instead.
/// </summary>
[Authorize(OrderingSvcPermissions.RestaurantOrders.Default)]
[RemoteService(IsEnabled = false)]
public class RestaurantOrderAppService : ApplicationService, IRestaurantOrderAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IDistributedEventBus _eventBus;

    public RestaurantOrderAppService(
        IRepository<Order, Guid> orderRepository,
        IDistributedEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    public async Task<PagedResultDto<OrderListDto>> GetListAsync(GetRestaurantOrdersInput input)
    {
        var queryable = await _orderRepository.GetQueryableAsync();

        queryable = queryable.Where(o => o.RestaurantId == input.RestaurantId);

        if (input.Status.HasValue)
        {
            var status = (DomainOrderStatus)(int)input.Status.Value;
            queryable = queryable.Where(o => o.Status == status);
        }

        if (input.FromDate.HasValue)
        {
            queryable = queryable.Where(o => o.CreationTime >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            queryable = queryable.Where(o => o.CreationTime <= input.ToDate.Value);
        }

        var totalCount = queryable.Count();

        queryable = queryable
            .OrderByDescending(o => o.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var orders = queryable.ToList();

        return new PagedResultDto<OrderListDto>(
            totalCount,
            ObjectMapper.Map<List<Order>, List<OrderListDto>>(orders));
    }

    public async Task<OrderDto> GetAsync(Guid restaurantId, Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId);

        if (order.RestaurantId != restaurantId)
        {
            throw new BusinessException("Order:NotForRestaurant");
        }

        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    [Authorize(OrderingSvcPermissions.RestaurantOrders.Accept)]
    public async Task AcceptAsync(Guid restaurantId, Guid orderId, int preparationMinutes)
    {
        var order = await _orderRepository.GetAsync(orderId);

        if (order.RestaurantId != restaurantId)
        {
            throw new BusinessException("Order:NotForRestaurant");
        }

        if (order.Status != DomainOrderStatus.Pending &&
            order.Status != DomainOrderStatus.PaymentConfirmed)
        {
            throw new BusinessException("Order:CannotAccept")
                .WithData("status", order.Status.ToString());
        }

        // Accept by starting preparation
        var previousStatus = MapToEventStatus(order.Status);
        order.StartPreparation();
        await _orderRepository.UpdateAsync(order);

        await _eventBus.PublishAsync(new OrderStatusChangedEto(
            order.Id,
            order.CustomerId,
            previousStatus,
            EventOrderStatus.Preparing,
            DateTime.UtcNow,
            null));
    }

    [Authorize(OrderingSvcPermissions.RestaurantOrders.Reject)]
    public async Task RejectAsync(Guid restaurantId, Guid orderId, string reason)
    {
        var order = await _orderRepository.GetAsync(orderId);

        if (order.RestaurantId != restaurantId)
        {
            throw new BusinessException("Order:NotForRestaurant");
        }

        order.Cancel(reason, OrderCancellationReason.RestaurantUnavailable);
        await _orderRepository.UpdateAsync(order);

        await _eventBus.PublishAsync(new OrderCancelledEto(
            order.Id,
            order.CustomerId,
            order.RiderId,
            OrderCancellationReason.RestaurantUnavailable,
            reason,
            DateTime.UtcNow));
    }

    [Authorize(OrderingSvcPermissions.RestaurantOrders.UpdateStatus)]
    public async Task StartPreparingAsync(Guid restaurantId, Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId);

        if (order.RestaurantId != restaurantId)
        {
            throw new BusinessException("Order:NotForRestaurant");
        }

        var previousStatus = MapToEventStatus(order.Status);
        order.StartPreparation();
        await _orderRepository.UpdateAsync(order);

        await _eventBus.PublishAsync(new OrderStatusChangedEto(
            order.Id,
            order.CustomerId,
            previousStatus,
            EventOrderStatus.Preparing,
            DateTime.UtcNow,
            null));
    }

    [Authorize(OrderingSvcPermissions.RestaurantOrders.UpdateStatus)]
    public async Task MarkReadyAsync(Guid restaurantId, Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId);

        if (order.RestaurantId != restaurantId)
        {
            throw new BusinessException("Order:NotForRestaurant");
        }

        order.MarkReadyForPickup();
        await _orderRepository.UpdateAsync(order);

        await _eventBus.PublishAsync(new OrderReadyForPickupEto(
            order.Id,
            order.RestaurantId,
            DateTime.UtcNow));
    }

    private static EventOrderStatus MapToEventStatus(DomainOrderStatus status)
    {
        return status switch
        {
            DomainOrderStatus.Pending => EventOrderStatus.Pending,
            DomainOrderStatus.PaymentConfirmed => EventOrderStatus.PaymentConfirmed,
            DomainOrderStatus.Preparing => EventOrderStatus.Preparing,
            DomainOrderStatus.ReadyForPickup => EventOrderStatus.ReadyForPickup,
            DomainOrderStatus.AwaitingPickup => EventOrderStatus.AwaitingRider,
            DomainOrderStatus.InTransit => EventOrderStatus.InTransit,
            DomainOrderStatus.Delivered => EventOrderStatus.Delivered,
            DomainOrderStatus.Completed => EventOrderStatus.Completed,
            DomainOrderStatus.Cancelled => EventOrderStatus.Cancelled,
            _ => EventOrderStatus.Pending
        };
    }
}
