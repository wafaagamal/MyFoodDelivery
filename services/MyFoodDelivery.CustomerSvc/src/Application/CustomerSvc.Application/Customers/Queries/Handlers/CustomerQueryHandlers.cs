using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerSvc.Domain.Customers;
using CustomerSvc.Domain.Customers.Events;
using MediatR;

namespace CustomerSvc.Application.Customers.Queries.Handlers;

/// <summary>
/// Handler for GetCustomerProfileQuery.
/// </summary>
public class GetCustomerProfileQueryHandler : IRequestHandler<GetCustomerProfileQuery, CustomerProfileDto?>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerProfileQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerProfileDto?> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.FindAsync(request.CustomerId, cancellationToken: cancellationToken);
        
        if (customer == null)
            return null;

        return MapToProfileDto(customer);
    }

    private static CustomerProfileDto MapToProfileDto(Customer customer)
    {
        return new CustomerProfileDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email.Value,
            customer.Phone?.Value,
            customer.ProfileImageUrl,
            customer.LoyaltyPoints,
            customer.LoyaltyTier.ToString(),
            customer.IsActive,
            customer.CreationTime,
            customer.LastOrderDate,
            customer.TotalOrders);
    }
}

/// <summary>
/// Handler for GetCustomerByEmailQuery.
/// </summary>
public class GetCustomerByEmailQueryHandler : IRequestHandler<GetCustomerByEmailQuery, CustomerProfileDto?>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByEmailQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerProfileDto?> Handle(GetCustomerByEmailQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.FindByEmailAsync(request.Email, cancellationToken);
        
        if (customer == null)
            return null;

        return new CustomerProfileDto(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email.Value,
            customer.Phone?.Value,
            customer.ProfileImageUrl,
            customer.LoyaltyPoints,
            customer.LoyaltyTier.ToString(),
            customer.IsActive,
            customer.CreationTime,
            customer.LastOrderDate,
            customer.TotalOrders);
    }
}

/// <summary>
/// Handler for GetCustomerAddressesQuery.
/// </summary>
public class GetCustomerAddressesQueryHandler : IRequestHandler<GetCustomerAddressesQuery, List<DeliveryAddressDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerAddressesQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<DeliveryAddressDto>> Handle(GetCustomerAddressesQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithAddressesAsync(request.CustomerId, cancellationToken);
        
        if (customer == null)
            return new List<DeliveryAddressDto>();

        return customer.Addresses
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new DeliveryAddressDto(
                a.Id,
                a.Label,
                a.Street,
                a.BuildingNumber,
                a.Floor,
                a.Apartment,
                a.City,
                a.District,
                a.PostalCode,
                a.Country,
                a.Coordinates?.Latitude,
                a.Coordinates?.Longitude,
                a.DeliveryInstructions,
                a.IsDefault,
                a.GetFullAddress()))
            .ToList();
    }
}

/// <summary>
/// Handler for GetDeliveryAddressQuery.
/// </summary>
public class GetDeliveryAddressQueryHandler : IRequestHandler<GetDeliveryAddressQuery, DeliveryAddressDto?>
{
    private readonly ICustomerRepository _customerRepository;

    public GetDeliveryAddressQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<DeliveryAddressDto?> Handle(GetDeliveryAddressQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithAddressesAsync(request.CustomerId, cancellationToken);
        
        var address = customer?.Addresses.FirstOrDefault(a => a.Id == request.AddressId && a.IsActive);
        
        if (address == null)
            return null;

        return new DeliveryAddressDto(
            address.Id,
            address.Label,
            address.Street,
            address.BuildingNumber,
            address.Floor,
            address.Apartment,
            address.City,
            address.District,
            address.PostalCode,
            address.Country,
            address.Coordinates?.Latitude,
            address.Coordinates?.Longitude,
            address.DeliveryInstructions,
            address.IsDefault,
            address.GetFullAddress());
    }
}

/// <summary>
/// Handler for GetCustomerPaymentMethodsQuery.
/// </summary>
public class GetCustomerPaymentMethodsQueryHandler : IRequestHandler<GetCustomerPaymentMethodsQuery, List<PaymentMethodDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerPaymentMethodsQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<List<PaymentMethodDto>> Handle(GetCustomerPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetWithPaymentMethodsAsync(request.CustomerId, cancellationToken);
        
        if (customer == null)
            return new List<PaymentMethodDto>();

        return customer.PaymentMethods
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.IsDefault)
            .ThenByDescending(p => p.CreatedAt)
            .Select(p => new PaymentMethodDto(
                p.Id,
                p.Type.ToString(),
                p.Label,
                p.Last4Digits,
                p.CardBrand,
                p.IsDefault,
                p.IsExpired,
                p.ExpiryDate,
                p.GetDisplayName()))
            .ToList();
    }
}

/// <summary>
/// Handler for GetLoyaltyInfoQuery.
/// </summary>
public class GetLoyaltyInfoQueryHandler : IRequestHandler<GetLoyaltyInfoQuery, LoyaltyInfoDto?>
{
    private readonly ICustomerRepository _customerRepository;
    private const decimal PointsToMoneyRatio = 0.01m; // 1 point = $0.01

    public GetLoyaltyInfoQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<LoyaltyInfoDto?> Handle(GetLoyaltyInfoQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.FindAsync(request.CustomerId, cancellationToken: cancellationToken);
        
        if (customer == null)
            return null;

        var nextTier = GetNextTier(customer.LoyaltyTier);
        var pointsToNextTier = customer.GetPointsToNextTier();
        var pointsValue = customer.LoyaltyPoints * PointsToMoneyRatio;

        return new LoyaltyInfoDto(
            customer.Id,
            customer.LoyaltyPoints,
            customer.LifetimeLoyaltyPoints,
            customer.LoyaltyTier.ToString(),
            nextTier?.ToString() ?? "Max",
            Math.Max(0, pointsToNextTier),
            pointsValue);
    }

    private static LoyaltyTier? GetNextTier(LoyaltyTier current)
    {
        return current switch
        {
            LoyaltyTier.Bronze => LoyaltyTier.Silver,
            LoyaltyTier.Silver => LoyaltyTier.Gold,
            LoyaltyTier.Gold => LoyaltyTier.Platinum,
            LoyaltyTier.Platinum => null,
            _ => null
        };
    }
}

/// <summary>
/// Handler for SearchCustomersQuery.
/// </summary>
public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, PagedResultDto<CustomerListItemDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public SearchCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResultDto<CustomerListItemDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        // For a production system, this would use a read-optimized query or a separate read model
        var customers = await _customerRepository.SearchAsync(
            request.SearchTerm ?? "",
            request.SkipCount,
            request.MaxResultCount,
            cancellationToken);

        var items = customers
            .Select(c => new CustomerListItemDto(
                c.Id,
                c.GetFullName(),
                c.Email.Value,
                c.LoyaltyTier.ToString(),
                c.LoyaltyPoints,
                c.IsActive,
                c.LastOrderDate,
                c.TotalOrders))
            .ToList();

        // Note: In production, you'd want a separate count query
        return new PagedResultDto<CustomerListItemDto>(items, items.Count);
    }
}
