using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OrderingSvc.Application.Orders.Commands;
using OrderingSvc.Domain.Orders;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;
using MyFoodDelivery.Shared.Events;
using DomainOrderStatus = OrderingSvc.Domain.Orders.OrderStatus;
using EventOrderStatus = MyFoodDelivery.Shared.Events.OrderStatus;
using DomainCancellationReason = OrderingSvc.Domain.Orders.OrderCancellationReason;

namespace OrderingSvc.Application.Orders.Handlers;

public class OrderCommandHandlers :
    IRequestHandler<CreateOrderCommand, CreateOrderResult>,
    IRequestHandler<ConfirmOrderPaymentCommand>,
    IRequestHandler<StartPreparationCommand>,
    IRequestHandler<MarkReadyForPickupCommand>,
    IRequestHandler<AssignRiderCommand>,
    IRequestHandler<MarkPickedUpCommand>,
    IRequestHandler<MarkDeliveredCommand>,
    IRequestHandler<CompleteOrderCommand>,
    IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDistributedEventBus _eventBus;

    public OrderCommandHandlers(
        IOrderRepository orderRepository,
        IDistributedEventBus eventBus)
    {
        _orderRepository = orderRepository;
        _eventBus = eventBus;
    }

    private static string GenerateOrderNumber() => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

    [UnitOfWork]
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderNumber = GenerateOrderNumber();
        
        var deliveryAddress = new DeliveryAddress(
            request.DeliveryAddress?.Street ?? "Default Street",
            request.DeliveryAddress?.BuildingNumber ?? "1",
            request.DeliveryAddress?.Apartment,
            request.DeliveryAddress?.Floor,
            request.DeliveryAddress?.City ?? "City",
            request.DeliveryAddress?.PostalCode ?? "00000",
            request.DeliveryAddress?.Country ?? "US",
            request.DeliveryAddress?.Latitude ?? 0,
            request.DeliveryAddress?.Longitude ?? 0,
            request.DeliveryAddress?.Instructions);

        var order = new Order(
            Guid.NewGuid(),
            request.CustomerId,
            request.RestaurantId,
            orderNumber,
            deliveryAddress,
            request.SpecialInstructions,
            request.DeliveryFee ?? 5.00m,
            0.50m, // Service fee
            30); // Estimated prep time

        foreach (var item in request.Items)
        {
            order.AddItem(
                item.MenuItemId,
                item.Name,
                item.Quantity,
                item.UnitPrice,
                item.SpecialInstructions);
        }

        await _orderRepository.InsertAsync(order, cancellationToken: cancellationToken);

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

        return new CreateOrderResult(
            order.Id,
            order.OrderNumber,
            order.TotalAmount,
            new Commands.PaymentResult(false, null, null, null));
    }

    [UnitOfWork]
    public async Task Handle(ConfirmOrderPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetAsync(request.OrderId, cancellationToken: cancellationToken);
        order.ConfirmPayment();
        await _orderRepository.UpdateAsync(order, cancellationToken: cancellationToken);
        
        await _eventBus.PublishAsync(new OrderPaymentConfirmedEto(
            order.Id,
            order.CustomerId,
            order.RestaurantId,
            DateTime.UtcNow));
    }

    [UnitOfWork]
    public async Task Handle(StartPreparationCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetAsync(request.OrderId, cancellationToken: cancellationToken);
        var previousStatus = MapToEventStatus(order.Status);
        order.StartPreparation();
        await _orderRepository.UpdateAsync(order, cancellationToken: cancellationToken);

        await _eventBus.PublishAsync(new OrderStatusChangedEto(
            order.Id,
            order.CustomerId,
            previousStatus,
            EventOrderStatus.Preparing,
            DateTime.UtcNow,
            null));
    }

    [UnitOfWork]
    public async Task Handle(MarkReadyForPickupCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetAsync(request.OrderId, cancellationToken: cancellationToken);
        order.MarkReadyForPickup();
        await _orderRepository.UpdateAsync(order, cancellationToken: cancellationToken);
    }

    [UnitOfWork]
    public async Task Handle(AssignRiderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetAsync(request.OrderId, cancellationToken: cancellationToken);
        order.AssignRider(request.RiderId, request.RiderName, 30); // Default 30 min estimate
        await _orderRepository.UpdateAsync(order, cancellationToken: cancellationToken);
    }

    [UnitOfWork]
    public async Task Handle(MarkPickedUpCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetAsync(request.OrderId, cancellationToken: cancellationToken);
        order.MarkPickedUp();
        await _orderRepository.UpdateAsync(order, cancellationToken: cancellationToken);
    }

    [UnitOfWork]
    public async Task Handle(MarkDeliveredCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetAsync(request.OrderId, cancellationToken: cancellationToken);
        order.MarkDelivered();
        await _orderRepository.UpdateAsync(order, cancellationToken: cancellationToken);
    }

    [UnitOfWork]
    public async Task Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetAsync(request.OrderId, cancellationToken: cancellationToken);
        order.Complete();
        await _orderRepository.UpdateAsync(order, cancellationToken: cancellationToken);
    }

    [UnitOfWork]
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetAsync(request.OrderId, cancellationToken: cancellationToken);
        var cancellationReason = request.CancelledBy.HasValue 
            ? DomainCancellationReason.CustomerRequested 
            : DomainCancellationReason.Other;
        order.Cancel(request.Reason ?? "No reason provided", cancellationReason);
        await _orderRepository.UpdateAsync(order, cancellationToken: cancellationToken);
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
