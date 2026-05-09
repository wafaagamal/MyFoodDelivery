using System.Threading.Tasks;
using CustomerSvc.Application.Customers.Commands;
using MediatR;
using Microsoft.Extensions.Logging;
using MyFoodDelivery.Shared.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Uow;

namespace CustomerSvc.BackgroundJobs.IntegrationEventHandlers;

/// <summary>
/// Handles UserCreatedEto from IdentityServer.
/// Creates a Customer aggregate when a new user with Customer role is created.
/// </summary>
public class UserCreatedEventHandler : IDistributedEventHandler<UserCreatedEto>, ITransientDependency
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        IMediator mediator,
        ILogger<UserCreatedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(UserCreatedEto eventData)
    {
        // Only handle Customer role users
        if (eventData.Role != UserRole.Customer)
        {
            _logger.LogDebug("Ignoring user creation for non-customer role: {Role}", eventData.Role);
            return;
        }

        _logger.LogInformation(
            "Processing UserCreatedEto for customer: {UserId}, Email: {Email}",
            eventData.UserId,
            eventData.Email);

        try
        {
            var command = new RegisterCustomerCommand(eventData.UserId);
            var customerId = await _mediator.Send(command);

            _logger.LogInformation(
                "Customer created successfully: {CustomerId} for user: {UserId}",
                customerId,
                eventData.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create customer for user: {UserId}. Error: {Error}",
                eventData.UserId,
                ex.Message);
            throw; // Rethrow to trigger retry via inbox pattern
        }
    }
}

/// <summary>
/// Handles OrderCompletedEto from OrderingSvc.
/// Awards loyalty points to the customer.
/// </summary>
public class OrderCompletedEventHandler : IDistributedEventHandler<OrderCompletedEto>, ITransientDependency
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderCompletedEventHandler> _logger;

    public OrderCompletedEventHandler(
        IMediator mediator,
        ILogger<OrderCompletedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task HandleEventAsync(OrderCompletedEto eventData)
    {
        _logger.LogInformation(
            "Processing OrderCompletedEto for order: {OrderId}, Customer: {CustomerId}, Points: {Points}",
            eventData.OrderId,
            eventData.CustomerId,
            eventData.LoyaltyPointsEarned);

        if (eventData.LoyaltyPointsEarned <= 0)
        {
            _logger.LogDebug("No loyalty points to award for order: {OrderId}", eventData.OrderId);
            return;
        }

        try
        {
            var command = new AddLoyaltyPointsCommand(
                eventData.CustomerId,
                eventData.LoyaltyPointsEarned,
                $"Order completed: {eventData.OrderId}");

            await _mediator.Send(command);

            _logger.LogInformation(
                "Loyalty points awarded: {Points} to customer: {CustomerId} for order: {OrderId}",
                eventData.LoyaltyPointsEarned,
                eventData.CustomerId,
                eventData.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to award loyalty points for order: {OrderId}. Error: {Error}",
                eventData.OrderId,
                ex.Message);
            throw;
        }
    }
}
