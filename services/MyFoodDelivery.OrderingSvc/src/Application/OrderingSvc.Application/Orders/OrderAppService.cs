using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderingSvc.Application.Contracts.Orders;
using OrderingSvc.Application.Contracts.Orders.Dtos;
using OrderingSvc.Application.Contracts.Permissions;
using OrderingSvc.Domain.Orders;
using OrderingSvc.Domain.Services;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using MyFoodDelivery.Shared.Events;
using DomainOrderStatus = OrderingSvc.Domain.Orders.OrderStatus;
using EventOrderStatus = MyFoodDelivery.Shared.Events.OrderStatus;

namespace OrderingSvc.Application.Orders;

/// <summary>
/// ABP Application Service for customer order management.
/// </summary>
[Authorize]
public class OrderAppService : ApplicationService, IOrderAppService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly ICartService _cartService;
    private readonly IDistributedEventBus _eventBus;

    public OrderAppService(
        IRepository<Order, Guid> orderRepository,
        ICartService cartService,
        IDistributedEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _cartService = cartService;
        _eventBus = eventBus;
    }

    public async Task<OrderDto> GetAsync(Guid id)
    {
        var queryable = (await _orderRepository.GetQueryableAsync())
            .Include(o => o.Items);
        var order = queryable.FirstOrDefault(o => o.Id == id);
        if (order == null)
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Order), id);
        EnsureOrderBelongsToCurrentUser(order);
        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    public async Task<OrderDto> GetByNumberAsync(string orderNumber)
    {
        var queryable = await _orderRepository.GetQueryableAsync();
        var order = queryable.FirstOrDefault(o => o.OrderNumber == orderNumber);

        if (order == null)
        {
            throw new BusinessException("Order:NotFound")
                .WithData("orderNumber", orderNumber);
        }

        EnsureOrderBelongsToCurrentUser(order);
        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    public async Task<PagedResultDto<OrderListDto>> GetListAsync(GetOrderListInput input)
    {
        var customerId = CurrentUser.Id ?? throw new BusinessException("Order:NotAuthenticated");
        var queryable = await _orderRepository.GetQueryableAsync();

        queryable = queryable.Where(o => o.CustomerId == customerId);

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

        var orders = queryable
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<OrderListDto>(
            totalCount,
            ObjectMapper.Map<List<Order>, List<OrderListDto>>(orders));
    }

    [Authorize]
    public async Task<OrderDto> CreateAsync(CreateOrderDto input)
    {
        var customerId = CurrentUser.Id ?? throw new BusinessException("Order:NotAuthenticated");

        // Get cart
        var cart = await _cartService.GetCartAsync(customerId);
        if (cart == null || !cart.Items.Any())
        {
            throw new BusinessException("Order:EmptyCart");
        }

        // Create delivery address value object
        var deliveryAddress = new DeliveryAddress(
            input.DeliveryAddress.Street,
            input.DeliveryAddress.BuildingNumber ?? "",
            input.DeliveryAddress.Apartment,
            input.DeliveryAddress.Floor,
            input.DeliveryAddress.City,
            "00000", // Default postal code
            "US", // Default country
            input.DeliveryAddress.Latitude,
            input.DeliveryAddress.Longitude,
            input.DeliveryAddress.DeliveryInstructions);

        // Generate order number
        var orderNumber = GenerateOrderNumber();

        // Create order
        var order = new Order(
            GuidGenerator.Create(),
            customerId,
            cart.RestaurantId,
            orderNumber,
            deliveryAddress,
            input.Notes,
            cart.DeliveryFee,
            cart.ServiceFee,
            30, // Default 30 min prep time
            (int)input.PaymentMethod,
            Guid.TryParse(input.PaymentMethodId, out var pmId) ? pmId : (Guid?)null,
            input.RestaurantName);

        // Add items from cart
        foreach (var item in cart.Items)
        {
            order.AddItem(
                item.MenuItemId,
                item.Name,
                item.Quantity,
                item.UnitPrice,
                item.SpecialInstructions);
        }

        // Apply discount and tip if any
        if (cart.Discount > 0)
        {
            order.ApplyDiscount(cart.Discount);
        }

        await _orderRepository.InsertAsync(order);

        // Clear cart after successful order
        await _cartService.ClearCartAsync(customerId);

        // Publish order created event
        var items = order.Items.Select(i => new OrderItemEto(
            i.MenuItemId,
            i.Name,
            i.Quantity,
            i.UnitPrice,
            i.SpecialInstructions)).ToList();

        await _eventBus.PublishAsync(new OrderCreatedEto(
            order.Id,
            order.CustomerId,
            order.RestaurantId,
            deliveryAddress.Street,
            deliveryAddress.City,
            deliveryAddress.PostalCode,
            deliveryAddress.Latitude,
            deliveryAddress.Longitude,
            order.TotalAmount,
            order.DeliveryFee,
            items,
            DateTime.UtcNow));

        return ObjectMapper.Map<Order, OrderDto>(order);
    }

    [Authorize]
    public async Task CancelAsync(Guid id, CancelOrderDto input)
    {
        var order = await _orderRepository.GetAsync(id);
        EnsureOrderBelongsToCurrentUser(order);

        // Only allow cancellation while Pending or PaymentConfirmed
        var cancellableStatuses = new[]
        {
            DomainOrderStatus.Pending,
            DomainOrderStatus.PaymentConfirmed
        };
        if (!cancellableStatuses.Contains(order.Status))
        {
            throw new BusinessException("Order:CannotCancel")
                .WithData("status", order.Status.ToString());
        }

        order.Cancel(input.Reason ?? "Customer requested cancellation", OrderCancellationReason.CustomerRequested);
        await _orderRepository.UpdateAsync(order);

        await _eventBus.PublishAsync(new OrderCancelledEto(
            order.Id,
            order.CustomerId,
            order.RiderId,
            OrderCancellationReason.CustomerRequested,
            input.Reason,
            DateTime.UtcNow));
    }

    public async Task<OrderTrackingDto> GetTrackingAsync(Guid id)
    {
        var queryable = (await _orderRepository.GetQueryableAsync())
            .Include(o => o.Items);
        var order = queryable.FirstOrDefault(o => o.Id == id);
        if (order == null)
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Order), id);
        EnsureOrderBelongsToCurrentUser(order);

        // Build status history from the order's lifecycle timestamps
        var history = new List<OrderStatusHistoryDto>
        {
            new() { Status = DomainOrderStatus.Pending, Timestamp = order.CreationTime, Note = "Order placed" }
        };
        if (order.PaymentConfirmedAt.HasValue)
            history.Add(new() { Status = DomainOrderStatus.PaymentConfirmed, Timestamp = order.PaymentConfirmedAt.Value, Note = "Payment confirmed" });
        if (order.PreparationStartedAt.HasValue)
            history.Add(new() { Status = DomainOrderStatus.Preparing, Timestamp = order.PreparationStartedAt.Value, Note = "Restaurant is preparing your order" });
        if (order.ReadyForPickupAt.HasValue)
            history.Add(new() { Status = DomainOrderStatus.ReadyForPickup, Timestamp = order.ReadyForPickupAt.Value, Note = "Ready for pickup" });
        if (order.PickedUpAt.HasValue)
            history.Add(new() { Status = DomainOrderStatus.InTransit, Timestamp = order.PickedUpAt.Value, Note = "Rider picked up your order" });
        if (order.DeliveredAt.HasValue)
            history.Add(new() { Status = DomainOrderStatus.Delivered, Timestamp = order.DeliveredAt.Value, Note = "Order delivered" });

        return new OrderTrackingDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            EstimatedDeliveryTime = order.EstimatedDeliveryTime,
            Restaurant = new RestaurantLocationDto
            {
                Id = order.RestaurantId,
                Name = null,
                Latitude = null,
                Longitude = null
            },
            DeliveryAddress = BuildDeliveryAddressDto(order),
            Rider = BuildRiderDto(order),
            StatusHistory = history
        };
    }

    private DeliveryAddressDto BuildDeliveryAddressDto(Order order)
    {
        var addr = order.DeliveryAddress;
        var lat = addr?.Latitude ?? 0;
        var lng = addr?.Longitude ?? 0;
        return new DeliveryAddressDto
        {
            Street = addr?.Street ?? string.Empty,
            BuildingNumber = addr?.BuildingNumber ?? string.Empty,
            Floor = addr?.Floor,
            Apartment = addr?.Apartment,
            City = addr?.City ?? string.Empty,
            District = null,
            Landmark = null,
            DeliveryInstructions = addr?.DeliveryInstructions,
            Latitude = lat,
            Longitude = lng
        };
    }

    private RiderTrackingDto? BuildRiderDto(Order order)
    {
        if (!order.RiderId.HasValue)
            return null;

        return new RiderTrackingDto
        {
            Id = order.RiderId.Value,
            Name = order.RiderName ?? "Rider",
            Phone = order.RiderPhone,
            CurrentLatitude = order.RiderLatitude,
            CurrentLongitude = order.RiderLongitude,
            EtaMinutes = order.EstimatedDeliveryMinutes
        };
    }

    public async Task RateAsync(RateOrderDto input)
    {
        var order = await _orderRepository.GetAsync(input.OrderId);
        EnsureOrderBelongsToCurrentUser(order);

        if (order.Status != DomainOrderStatus.Completed && 
            order.Status != DomainOrderStatus.Delivered)
        {
            throw new BusinessException("Order:CannotRateYet");
        }

        // Rating would typically be stored separately
        // For now, just acknowledge the rating
        await _orderRepository.UpdateAsync(order);
    }

    private void EnsureOrderBelongsToCurrentUser(Order order)
    {
        var currentUserId = CurrentUser.Id ?? throw new BusinessException("Order:NotAuthenticated");
        if (order.CustomerId != currentUserId)
        {
            throw new BusinessException("Order:NotYours");
        }
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }
}
