using System;
using System.Collections.Generic;
using CustomerSvc.Application.Contracts.Customers.Dtos;
using MediatR;

namespace CustomerSvc.Application.Customers.Queries;

/// <summary>
/// Query to get a customer's profile by ID.
/// Name/email come from JWT claims — not stored in CustomerSvc.
/// </summary>
public record GetCustomerProfileQuery(Guid CustomerId) : IRequest<CustomerProfileDto?>;

/// <summary>
/// Query to get a customer's delivery addresses.
/// </summary>
public record GetCustomerAddressesQuery(Guid CustomerId) : IRequest<List<DeliveryAddressDto>>;

/// <summary>
/// Query to get a specific delivery address.
/// </summary>
public record GetDeliveryAddressQuery(Guid CustomerId, Guid AddressId) : IRequest<DeliveryAddressDto?>;

/// <summary>
/// Query to get a customer's payment methods.
/// </summary>
public record GetCustomerPaymentMethodsQuery(Guid CustomerId) : IRequest<List<PaymentMethodDto>>;

/// <summary>
/// Query to get a customer's loyalty information.
/// </summary>
public record GetLoyaltyInfoQuery(Guid CustomerId) : IRequest<LoyaltyInfoDto?>;

/// <summary>
/// Query to search customers (admin functionality).
/// </summary>
public record SearchCustomersQuery(
    string? SearchTerm,
    string? LoyaltyTier,
    bool? IsActive,
    int SkipCount = 0,
    int MaxResultCount = 20) : IRequest<CustomerPagedResultDto<CustomerListItemDto>>;
