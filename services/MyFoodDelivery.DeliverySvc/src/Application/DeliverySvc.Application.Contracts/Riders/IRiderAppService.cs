using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeliverySvc.Application.Contracts.Riders.Dtos;
using DeliverySvc.Domain.Riders;

namespace DeliverySvc.Application.Contracts.Riders;

public interface IRiderAppService
{
    Task<RiderDto> GetAsync(Guid id);
    Task<List<RiderListDto>> GetListAsync(bool? isActive = null);
    Task<List<RiderListDto>> GetAvailableRidersAsync();
    Task<RiderDto> CreateAsync(Guid userId);
    Task<RiderDto> UpdateVehicleAsync(Guid id, UpdateRiderVehicleDto input);
    Task UpdateStatusAsync(Guid id, RiderStatus status);
    Task UpdateLocationAsync(Guid id, UpdateRiderLocationDto input);
}
