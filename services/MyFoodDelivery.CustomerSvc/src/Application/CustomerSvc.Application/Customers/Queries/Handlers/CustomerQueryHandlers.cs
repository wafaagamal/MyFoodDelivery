using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerSvc.Application.Customers.Queries;
using CustomerSvc.Application.Contracts.Customers.Dtos;
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
            customer.LoyaltyPoints,
            customer.LoyaltyTier.ToString(),
            customer.IsActive,
            customer.CreationTime,
            customer.LastOrderDate,
            customer.TotalOrders,
            customer.PhoneNumber,
            customer.ProfileImageUrl);
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
public class SearchCustomersQueryHandler : IRequestHandler<SearchCustomersQuery, CustomerPagedResultDto<CustomerListItemDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public SearchCustomersQueryHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerPagedResultDto<CustomerListItemDto>> Handle(SearchCustomersQuery request, CancellationToken cancellationToken)
    {
        var dbContext = await GetSearchableContextAsync(request);
        _ = dbContext; // placeholder — query by loyalty tier/active status only (no name/email)

        // Search by Id range is no longer meaningful without name/email;
        // return all matching by IsActive filter using repository
        var tier = request.LoyaltyTier != null
            ? Enum.TryParse<LoyaltyTier>(request.LoyaltyTier, out var t) ? t : (LoyaltyTier?)null
            : null;

        List<Customer> customers;
        if (tier.HasValue)
            customers = await _customerRepository.GetByLoyaltyTierAsync(tier.Value, request.SkipCount, request.MaxResultCount, cancellationToken);
        else
            customers = await _customerRepository.GetListAsync(cancellationToken: cancellationToken);

        var items = customers
            .Where(c => !request.IsActive.HasValue || c.IsActive == request.IsActive.Value)
            .Select(c => new CustomerListItemDto(
                c.Id,
                c.LoyaltyTier.ToString(),
                c.LoyaltyPoints,
                c.IsActive,
                c.LastOrderDate,
                c.TotalOrders))
            .ToList();

        return new CustomerPagedResultDto<CustomerListItemDto>(items, items.Count);
    }

    private Task<object?> GetSearchableContextAsync(SearchCustomersQuery _) => Task.FromResult<object?>(null);
}
