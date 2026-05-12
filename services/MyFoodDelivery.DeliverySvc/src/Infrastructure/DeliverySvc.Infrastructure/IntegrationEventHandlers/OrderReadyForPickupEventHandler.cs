using DeliverySvc.Domain.DeliveryTasks;
using Microsoft.Extensions.Logging;
using MyFoodDelivery.Shared.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace DeliverySvc.Infrastructure.IntegrationEventHandlers;

/// <summary>
/// Creates a DeliveryTask when an order is marked ready for pickup by the restaurant.
/// Idempotent — skips creation if a task already exists for the order.
/// </summary>
public class OrderReadyForPickupEventHandler : IDistributedEventHandler<OrderReadyForPickupEto>, ITransientDependency
{
    private readonly IRepository<DeliveryTask, Guid> _taskRepository;
    private readonly ILogger<OrderReadyForPickupEventHandler> _logger;

    public OrderReadyForPickupEventHandler(
        IRepository<DeliveryTask, Guid> taskRepository,
        ILogger<OrderReadyForPickupEventHandler> logger)
    {
        _taskRepository = taskRepository;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(OrderReadyForPickupEto eventData)
    {
        _logger.LogInformation(
            "OrderReadyForPickupEventHandler: processing orderId={OrderId}", eventData.OrderId);

        try
        {
            // Idempotency: skip if task already exists for this order
            var query = await _taskRepository.GetQueryableAsync();
            var existing = query.FirstOrDefault(t => t.OrderId == eventData.OrderId);
            if (existing != null)
            {
                _logger.LogInformation(
                    "DeliveryTask already exists for orderId={OrderId}, taskId={TaskId}. Skipping.",
                    eventData.OrderId, existing.Id);
                return;
            }

            var task = new DeliveryTask(
                Guid.NewGuid(),
                eventData.OrderId,
                eventData.RestaurantId,
                string.IsNullOrWhiteSpace(eventData.PickupAddressLine)
                    ? eventData.RestaurantName
                    : eventData.PickupAddressLine,
                eventData.PickupLatitude,
                eventData.PickupLongitude,
                string.IsNullOrWhiteSpace(eventData.DeliveryAddressLine)
                    ? "Customer Address"
                    : eventData.DeliveryAddressLine,
                eventData.DeliveryLatitude,
                eventData.DeliveryLongitude,
                eventData.DeliveryInstructions,
                eventData.EstimatedMinutes);

            await _taskRepository.InsertAsync(task);

            _logger.LogInformation(
                "DeliveryTask created: taskId={TaskId} for orderId={OrderId}",
                task.Id, eventData.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create DeliveryTask for orderId={OrderId}: {Error}",
                eventData.OrderId, ex.Message);
            throw;
        }
    }
}
