using System;
using System.Threading.Tasks;
using DeliverySvc.Application.Contracts.DeliveryTasks.Dtos;

namespace DeliverySvc.Application.Contracts.DeliveryTasks;

public interface IDeliveryTaskAppService
{
    Task<DeliveryTaskDto> GetAsync(Guid id);
    Task<DeliveryTaskDto> GetByOrderAsync(Guid orderId);
    Task<DeliveryTaskDto> CreateAsync(CreateDeliveryTaskDto input);
    Task<DeliveryTaskDto> AssignRiderAsync(Guid taskId, AssignRiderDto input);
    Task MarkPickedUpAsync(Guid taskId);
    Task MarkDeliveredAsync(Guid taskId);
}
