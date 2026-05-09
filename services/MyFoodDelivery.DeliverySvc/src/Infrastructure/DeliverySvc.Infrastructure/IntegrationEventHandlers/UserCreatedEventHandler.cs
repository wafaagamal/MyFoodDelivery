using DeliverySvc.Domain.Riders;
using Microsoft.Extensions.Logging;
using MyFoodDelivery.Shared.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace DeliverySvc.Infrastructure.IntegrationEventHandlers;

/// <summary>
/// Handles UserCreatedEto from AuthSvc.
/// Creates a Rider aggregate when a new user with Rider role registers.
/// </summary>
public class UserCreatedEventHandler : IDistributedEventHandler<UserCreatedEto>, ITransientDependency
{
    private readonly IRepository<Rider, Guid> _riderRepository;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        IRepository<Rider, Guid> riderRepository,
        ILogger<UserCreatedEventHandler> logger)
    {
        _riderRepository = riderRepository;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(UserCreatedEto eventData)
    {
        if (eventData.Role != UserRole.Rider)
        {
            _logger.LogDebug("Ignoring user creation for non-rider role: {Role}", eventData.Role);
            return;
        }

        _logger.LogInformation(
            "Processing UserCreatedEto for rider: {UserId}", eventData.UserId);

        try
        {
            // Idempotency: rider might already exist
            var existing = await _riderRepository.FindAsync(eventData.UserId);
            if (existing != null)
            {
                _logger.LogInformation("Rider already exists for UserId {UserId}", eventData.UserId);
                return;
            }

            // Id = UserId — same GUID, no FK, consistent with Customer pattern
            var rider = new Rider(eventData.UserId);
            await _riderRepository.InsertAsync(rider);

            _logger.LogInformation(
                "Rider created successfully: {RiderId}", rider.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create Rider for user: {UserId}. Error: {Error}",
                eventData.UserId,
                ex.Message);
            throw;
        }
    }
}
