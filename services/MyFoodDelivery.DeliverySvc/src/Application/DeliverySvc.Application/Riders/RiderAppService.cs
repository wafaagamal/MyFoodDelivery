using DeliverySvc.Application.Contracts.Riders;
using DeliverySvc.Application.Contracts.Riders.Dtos;
using DeliverySvc.Domain.Riders;
using DeliverySvc.Infrastructure.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace DeliverySvc.Application.Riders;

public class RiderAppService : ApplicationService, IRiderAppService
{
    private readonly IRepository<Rider, Guid> _riderRepository;

    public RiderAppService(IRepository<Rider, Guid> riderRepository)
    {
        _riderRepository = riderRepository;
    }

    public async Task<RiderDto> GetAsync(Guid id)
    {
        var rider = await _riderRepository.GetAsync(id);
        return MapToDto(rider);
    }

    public async Task<List<RiderListDto>> GetListAsync(bool? isActive = null)
    {
        var query = await _riderRepository.GetQueryableAsync();
        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);
        var riders = await query.OrderByDescending(r => r.AverageRating).ToListAsync();
        return riders.Select(MapToListDto).ToList();
    }

    public async Task<List<RiderListDto>> GetAvailableRidersAsync()
    {
        var query = await _riderRepository.GetQueryableAsync();
        var riders = await query
            .Where(r => r.IsActive && r.Status == RiderStatus.Available)
            .OrderByDescending(r => r.AverageRating)
            .ToListAsync();
        return riders.Select(MapToListDto).ToList();
    }

    /// <summary>
    /// Creates a rider record for an existing IdentityUser.
    /// UserId must correspond to an existing AbpUsers.Id in AuthSvc.
    /// </summary>
    public async Task<RiderDto> CreateAsync(Guid userId)
    {
        var existing = await _riderRepository.FindAsync(userId);
        if (existing != null)
            return MapToDto(existing);

        var rider = new Rider(userId);
        await _riderRepository.InsertAsync(rider);
        return MapToDto(rider);
    }

    public async Task<RiderDto> UpdateVehicleAsync(Guid id, UpdateRiderVehicleDto input)
    {
        var rider = await _riderRepository.GetAsync(id);
        rider.UpdateVehicle(input.VehicleType, input.VehiclePlate);
        await _riderRepository.UpdateAsync(rider);
        return MapToDto(rider);
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

    private static RiderDto MapToDto(Rider r) => new()
    {
        Id = r.Id, VehicleType = r.VehicleType, VehiclePlate = r.VehiclePlate,
        Status = r.Status, IsOnline = r.IsOnline, Latitude = r.Latitude, Longitude = r.Longitude,
        LastLocationUpdate = r.LastLocationUpdate, TotalDeliveries = r.TotalDeliveries,
        TotalEarnings = r.TotalEarnings, AverageRating = r.AverageRating,
        RatingCount = r.RatingCount, IsActive = r.IsActive
    };

    private static RiderListDto MapToListDto(Rider r) => new()
    {
        Id = r.Id, Status = r.Status, IsOnline = r.IsOnline,
        AverageRating = r.AverageRating, TotalDeliveries = r.TotalDeliveries, IsActive = r.IsActive
    };
}
