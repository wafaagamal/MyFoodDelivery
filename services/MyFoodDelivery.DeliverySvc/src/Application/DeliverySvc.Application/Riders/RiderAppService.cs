using DeliverySvc.Application.Contracts.Riders;
using DeliverySvc.Domain.Riders;
using DeliverySvc.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace DeliverySvc.Application.Riders;

public class RiderAppService : ApplicationService
{
    private readonly IRepository<Rider, Guid> _riderRepository;

    public RiderAppService(IRepository<Rider, Guid> riderRepository)
    {
        _riderRepository = riderRepository;
    }

    public async Task<RiderDto> GetAsync(Guid id)
    {
        var rider = await _riderRepository.GetAsync(id);
        return ObjectMapper.Map<Rider, RiderDto>(rider);
    }

    public async Task<List<RiderListDto>> GetListAsync(bool? isActive = null)
    {
        var query = await _riderRepository.GetQueryableAsync();

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        var riders = await query.OrderBy(r => r.FirstName).ToListAsync();
        return ObjectMapper.Map<List<Rider>, List<RiderListDto>>(riders);
    }

    public async Task<List<RiderListDto>> GetAvailableRidersAsync()
    {
        var query = await _riderRepository.GetQueryableAsync();
        var riders = await query
            .Where(r => r.IsActive && r.Status == RiderStatus.Available)
            .OrderByDescending(r => r.Rating)
            .ToListAsync();
        return ObjectMapper.Map<List<Rider>, List<RiderListDto>>(riders);
    }

    public async Task<RiderDto> CreateAsync(CreateRiderDto input)
    {
        var rider = new Rider(
            GuidGenerator.Create(),
            input.FirstName,
            input.LastName,
            input.PhoneNumber,
            input.Email,
            input.VehicleType,
            input.VehiclePlate);

        await _riderRepository.InsertAsync(rider);
        return ObjectMapper.Map<Rider, RiderDto>(rider);
    }

    public async Task<RiderDto> UpdateAsync(Guid id, UpdateRiderDto input)
    {
        var rider = await _riderRepository.GetAsync(id);
        rider.UpdateProfile(input.FirstName, input.LastName, input.Email, input.VehicleType, input.VehiclePlate);
        await _riderRepository.UpdateAsync(rider);
        return ObjectMapper.Map<Rider, RiderDto>(rider);
    }

    public async Task UpdateStatusAsync(Guid id, RiderStatus status)
    {
        var rider = await _riderRepository.GetAsync(id);
        rider.UpdateStatus(status);
        await _riderRepository.UpdateAsync(rider);
    }

    public async Task UpdateLocationAsync(Guid id, UpdateRiderLocationDto input)
    {
        var rider = await _riderRepository.GetAsync(id);
        rider.UpdateLocation(input.Latitude, input.Longitude);
        await _riderRepository.UpdateAsync(rider);
    }
}
