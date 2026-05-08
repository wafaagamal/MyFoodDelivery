using System.Threading.Tasks;
using CustomerSvc.Domain.Customers.Events;
using Microsoft.Extensions.Logging;
using MyFoodDelivery.Shared.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace CustomerSvc.BackgroundJobs.DomainEventHandlers;

/// <summary>
/// Handles CustomerCreatedDomainEvent and publishes CustomerRegisteredEto to the event bus.
/// </summary>
public class CustomerCreatedDomainEventHandler : 
    ILocalEventHandler<EntityCreatedEventData<Domain.Customers.Customer>>,
    ITransientDependency
{
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly ILogger<CustomerCreatedDomainEventHandler> _logger;

    public CustomerCreatedDomainEventHandler(
        IDistributedEventBus distributedEventBus,
        ILogger<CustomerCreatedDomainEventHandler> logger)
    {
        _distributedEventBus = distributedEventBus;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(EntityCreatedEventData<Domain.Customers.Customer> eventData)
    {
        var customer = eventData.Entity;

        _logger.LogInformation(
            "Publishing CustomerRegisteredEto for customer: {CustomerId}",
            customer.Id);

        await _distributedEventBus.PublishAsync(new CustomerRegisteredEto(
            customer.Id,
            customer.Email.Value,
            customer.FirstName,
            customer.LastName,
            customer.Phone?.Value));
    }
}

/// <summary>
/// Handles CustomerAddressAddedDomainEvent and publishes CustomerAddressAddedEto.
/// </summary>
public class CustomerAddressAddedDomainEventHandler :
    ILocalEventHandler<CustomerAddressAddedDomainEvent>,
    ITransientDependency
{
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly ILogger<CustomerAddressAddedDomainEventHandler> _logger;

    public CustomerAddressAddedDomainEventHandler(
        IDistributedEventBus distributedEventBus,
        ILogger<CustomerAddressAddedDomainEventHandler> logger)
    {
        _distributedEventBus = distributedEventBus;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(CustomerAddressAddedDomainEvent eventData)
    {
        _logger.LogInformation(
            "Publishing CustomerAddressAddedEto for customer: {CustomerId}, Address: {AddressId}",
            eventData.CustomerId,
            eventData.AddressId);

        await _distributedEventBus.PublishAsync(new CustomerAddressAddedEto(
            eventData.CustomerId,
            eventData.AddressId,
            eventData.Street,
            eventData.City,
            eventData.PostalCode,
            eventData.Country,
            eventData.IsDefault));
    }
}

/// <summary>
/// Handles LoyaltyPointsEarnedDomainEvent and publishes LoyaltyPointsUpdatedEto.
/// </summary>
public class LoyaltyPointsChangedDomainEventHandler :
    ILocalEventHandler<LoyaltyPointsEarnedDomainEvent>,
    ILocalEventHandler<LoyaltyPointsSpentDomainEvent>,
    ITransientDependency
{
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly ILogger<LoyaltyPointsChangedDomainEventHandler> _logger;

    public LoyaltyPointsChangedDomainEventHandler(
        IDistributedEventBus distributedEventBus,
        ILogger<LoyaltyPointsChangedDomainEventHandler> logger)
    {
        _distributedEventBus = distributedEventBus;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(LoyaltyPointsEarnedDomainEvent eventData)
    {
        _logger.LogInformation(
            "Publishing LoyaltyPointsUpdatedEto (earned) for customer: {CustomerId}, Points: +{Points}",
            eventData.CustomerId,
            eventData.PointsEarned);

        var reason = eventData.Reason.Contains("Order") 
            ? LoyaltyPointsChangeReason.OrderCompleted 
            : LoyaltyPointsChangeReason.Promotion;

        await _distributedEventBus.PublishAsync(new LoyaltyPointsUpdatedEto(
            eventData.CustomerId,
            eventData.PointsEarned,
            eventData.NewTotal,
            reason));
    }

    [UnitOfWork]
    public async Task HandleEventAsync(LoyaltyPointsSpentDomainEvent eventData)
    {
        _logger.LogInformation(
            "Publishing LoyaltyPointsUpdatedEto (spent) for customer: {CustomerId}, Points: -{Points}",
            eventData.CustomerId,
            eventData.PointsSpent);

        await _distributedEventBus.PublishAsync(new LoyaltyPointsUpdatedEto(
            eventData.CustomerId,
            -eventData.PointsSpent,
            eventData.NewTotal,
            LoyaltyPointsChangeReason.Redemption));
    }
}

/// <summary>
/// Handles CustomerDeactivatedDomainEvent and publishes CustomerDeactivatedEto.
/// </summary>
public class CustomerDeactivatedDomainEventHandler :
    ILocalEventHandler<CustomerDeactivatedDomainEvent>,
    ITransientDependency
{
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly ILogger<CustomerDeactivatedDomainEventHandler> _logger;

    public CustomerDeactivatedDomainEventHandler(
        IDistributedEventBus distributedEventBus,
        ILogger<CustomerDeactivatedDomainEventHandler> logger)
    {
        _distributedEventBus = distributedEventBus;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(CustomerDeactivatedDomainEvent eventData)
    {
        _logger.LogInformation(
            "Publishing CustomerDeactivatedEto for customer: {CustomerId}",
            eventData.CustomerId);

        await _distributedEventBus.PublishAsync(new CustomerDeactivatedEto(
            eventData.CustomerId,
            eventData.OccurredAt));
    }
}
