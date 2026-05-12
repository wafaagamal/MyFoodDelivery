using System;
using System.Threading.Tasks;
using MyFoodDelivery.Shared.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;
using DomainOrder = OrderingSvc.Domain.Orders.Order;
using DomainOrderStatus = OrderingSvc.Domain.Orders.OrderStatus;

namespace OrderingSvc.Application.Orders;

/// <summary>
/// Handles OrderDeliveredEto from DeliverySvc.
/// Advances the Order through AssignRider → MarkPickedUp → MarkDelivered.
/// </summary>
public class OrderDeliveredEventHandler
    : IDistributedEventHandler<OrderDeliveredEto>, ITransientDependency
{
    private readonly IRepository<DomainOrder, Guid> _orderRepository;

    public OrderDeliveredEventHandler(IRepository<DomainOrder, Guid> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(OrderDeliveredEto eventData)
    {
        var order = await _orderRepository.FindAsync(eventData.OrderId);
        if (order == null) return;

        // Advance through required states to reach Delivered
        if (order.Status == DomainOrderStatus.ReadyForPickup)
        {
            order.AssignRider(eventData.RiderId, "Rider", null, 0);
        }

        if (order.Status == DomainOrderStatus.AwaitingPickup || order.Status == DomainOrderStatus.ReadyForPickup)
        {
            order.MarkPickedUp();
        }

        if (order.Status == DomainOrderStatus.InTransit)
        {
            order.MarkDelivered();
        }

        await _orderRepository.UpdateAsync(order);
    }
}

