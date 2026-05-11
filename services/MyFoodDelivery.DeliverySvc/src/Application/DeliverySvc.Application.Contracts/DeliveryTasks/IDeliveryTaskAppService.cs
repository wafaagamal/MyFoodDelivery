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
    Task<DeliveryTaskDto> AcceptAsync(Guid taskId, Guid riderId);
    Task<List<DeliveryTaskDto>> GetAvailableAsync();
    Task<DeliveryTaskDto?> GetActiveForRiderAsync(Guid riderId);
    Task<List<DeliveryTaskDto>> GetMyTasksAsync(Guid riderId, int skip, int take);
    Task MarkPickedUpAsync(Guid taskId);
    Task MarkDeliveredAsync(Guid taskId);
}
