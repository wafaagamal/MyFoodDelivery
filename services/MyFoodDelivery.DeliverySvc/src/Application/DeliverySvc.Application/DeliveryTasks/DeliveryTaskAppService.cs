using DeliverySvc.Application.Contracts.DeliveryTasks;
using DeliverySvc.Application.Contracts.DeliveryTasks.Dtos;
using DeliverySvc.Domain.DeliveryTasks;
using DeliverySvc.Domain.Riders;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DeliverySvc.Application.DeliveryTasks;

public class DeliveryTaskAppService : ApplicationService, IDeliveryTaskAppService
{
    private readonly IRepository<DeliveryTask, Guid> _taskRepository;
    private readonly IRepository<Rider, Guid> _riderRepository;

    public DeliveryTaskAppService(
        IRepository<DeliveryTask, Guid> taskRepository,
        IRepository<Rider, Guid> riderRepository)
    {
        _taskRepository = taskRepository;
        _riderRepository = riderRepository;
    }

    public async Task<DeliveryTaskDto> GetAsync(Guid id)
    {
        var task = await _taskRepository.GetAsync(id);
        var dto = ObjectMapper.Map<DeliveryTask, DeliveryTaskDto>(task);
        return dto;
    }

    public async Task<DeliveryTaskDto> GetByOrderAsync(Guid orderId)
    {
        var query = await _taskRepository.GetQueryableAsync();
        var task = await query.FirstOrDefaultAsync(t => t.OrderId == orderId)
            ?? throw new Exception($"No delivery task found for order {orderId}");
        return ObjectMapper.Map<DeliveryTask, DeliveryTaskDto>(task);
    }

    public async Task<DeliveryTaskDto> CreateAsync(CreateDeliveryTaskDto input)
    {
        var task = new DeliveryTask(
            GuidGenerator.Create(),
            input.OrderId,
            input.RestaurantId,
            input.PickupAddress,
            input.PickupLatitude,
            input.PickupLongitude,
            input.DeliveryAddress,
            input.DeliveryLatitude,
            input.DeliveryLongitude,
            input.DeliveryInstructions,
            input.EstimatedMinutes);

        await _taskRepository.InsertAsync(task);
        return ObjectMapper.Map<DeliveryTask, DeliveryTaskDto>(task);
    }

    public async Task<DeliveryTaskDto> AssignRiderAsync(Guid taskId, AssignRiderDto input)
    {
        var task = await _taskRepository.GetAsync(taskId);
        var rider = await _riderRepository.GetAsync(input.RiderId);

        task.AssignRider(input.RiderId);
        rider.UpdateStatus(RiderStatus.OnDelivery);

        await _taskRepository.UpdateAsync(task);
        await _riderRepository.UpdateAsync(rider);

        return ObjectMapper.Map<DeliveryTask, DeliveryTaskDto>(task);
    }

    public async Task MarkPickedUpAsync(Guid taskId)
    {
        var task = await _taskRepository.GetAsync(taskId);
        task.MarkPickedUp();
        await _taskRepository.UpdateAsync(task);
    }

    public async Task MarkDeliveredAsync(Guid taskId)
    {
        var task = await _taskRepository.GetAsync(taskId);
        task.MarkDelivered();

        if (task.RiderId.HasValue)
        {
            var rider = await _riderRepository.FindAsync(task.RiderId.Value);
            if (rider != null)
            {
                rider.UpdateStatus(RiderStatus.Available);
                rider.RecordDelivery(5.0m, 5.0);
                await _riderRepository.UpdateAsync(rider);
            }
        }

        await _taskRepository.UpdateAsync(task);
    }
}
