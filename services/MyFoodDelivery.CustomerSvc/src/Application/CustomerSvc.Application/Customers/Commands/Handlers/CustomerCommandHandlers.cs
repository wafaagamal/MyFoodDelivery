using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerSvc.Domain.Customers;
using CustomerSvc.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace CustomerSvc.Application.Customers.Commands.Handlers;

/// <summary>
/// Handler for RegisterCustomerCommand.
/// Creates a new Customer aggregate when a user is created in IdentityServer.
/// </summary>
public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<RegisterCustomerCommandHandler> _logger;

    public RegisterCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ILogger<RegisterCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    [UnitOfWork]
    public async Task<Guid> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        // Idempotency check - customer might already exist
        var existing = await _customerRepository.FindByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
        {
            _logger.LogInformation("Customer already exists for email {Email}, returning existing ID {CustomerId}", 
                request.Email, existing.Id);
            return existing.Id;
        }

        var email = new Email(request.Email);
        var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) 
            ? null 
            : new PhoneNumber(request.PhoneNumber);

        var customer = new Customer(
            request.UserId,
            request.FirstName,
            request.LastName,
            email,
            phone);

        await _customerRepository.InsertAsync(customer, autoSave: true, cancellationToken: cancellationToken);

        _logger.LogInformation("Customer {CustomerId} created for email {Email}", customer.Id, request.Email);

        return customer.Id;
    }
}

/// <summary>
/// Handler for UpdateCustomerProfileCommand.
/// </summary>
public class UpdateCustomerProfileCommandHandler : IRequestHandler<UpdateCustomerProfileCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public UpdateCustomerProfileCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(UpdateCustomerProfileCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetAsync(request.CustomerId, cancellationToken: cancellationToken);

        var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) 
            ? null 
            : new PhoneNumber(request.PhoneNumber);

        customer.UpdateProfile(request.FirstName, request.LastName, phone);

        if (request.ProfileImageUrl != null)
        {
            customer.SetProfileImage(request.ProfileImageUrl);
        }

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler for AddDeliveryAddressCommand.
/// </summary>
public class AddDeliveryAddressCommandHandler : IRequestHandler<AddDeliveryAddressCommand, Guid>
{
    private readonly ICustomerRepository _customerRepository;

    public AddDeliveryAddressCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task<Guid> Handle(AddDeliveryAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithAddressesAsync(request.CustomerId, cancellationToken)
            ?? throw new BusinessException("Customer:NotFound");

        GeoCoordinate? coordinates = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            coordinates = new GeoCoordinate(request.Latitude.Value, request.Longitude.Value);
        }

        var addressId = customer.AddDeliveryAddress(
            request.Label,
            request.Street,
            request.BuildingNumber,
            request.Floor,
            request.Apartment,
            request.City,
            request.District,
            request.PostalCode,
            request.Country,
            coordinates,
            request.DeliveryInstructions,
            request.IsDefault);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);

        return addressId;
    }
}

/// <summary>
/// Handler for UpdateDeliveryAddressCommand.
/// </summary>
public class UpdateDeliveryAddressCommandHandler : IRequestHandler<UpdateDeliveryAddressCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public UpdateDeliveryAddressCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(UpdateDeliveryAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithAddressesAsync(request.CustomerId, cancellationToken)
            ?? throw new BusinessException("Customer:NotFound");

        GeoCoordinate? coordinates = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            coordinates = new GeoCoordinate(request.Latitude.Value, request.Longitude.Value);
        }

        customer.UpdateDeliveryAddress(
            request.AddressId,
            request.Label,
            request.Street,
            request.BuildingNumber,
            request.Floor,
            request.Apartment,
            request.City,
            request.District,
            request.PostalCode,
            request.Country,
            coordinates,
            request.DeliveryInstructions);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler for RemoveDeliveryAddressCommand.
/// </summary>
public class RemoveDeliveryAddressCommandHandler : IRequestHandler<RemoveDeliveryAddressCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public RemoveDeliveryAddressCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(RemoveDeliveryAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithAddressesAsync(request.CustomerId, cancellationToken)
            ?? throw new BusinessException("Customer:NotFound");

        customer.RemoveDeliveryAddress(request.AddressId);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}

/// <summary>
/// Handler for SetDefaultAddressCommand.
/// </summary>
public class SetDefaultAddressCommandHandler : IRequestHandler<SetDefaultAddressCommand>
{
    private readonly ICustomerRepository _customerRepository;

    public SetDefaultAddressCommandHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    [UnitOfWork]
    public async Task Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithAddressesAsync(request.CustomerId, cancellationToken)
            ?? throw new BusinessException("Customer:NotFound");

        customer.SetDefaultAddress(request.AddressId);

        await _customerRepository.UpdateAsync(customer, autoSave: true, cancellationToken: cancellationToken);
    }
}
