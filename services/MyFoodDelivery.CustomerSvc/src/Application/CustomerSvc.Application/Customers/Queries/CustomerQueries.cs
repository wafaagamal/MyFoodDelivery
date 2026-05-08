using System;
using System.Collections.Generic;
using MediatR;

namespace CustomerSvc.Application.Customers.Queries;

/// <summary>
/// Query to get a customer's profile by ID.
/// </summary>
public record GetCustomerProfileQuery(Guid CustomerId) : IRequest<CustomerProfileDto?>;

/// <summary>
/// Query to get a customer's profile by email.
/// </summary>
public record GetCustomerByEmailQuery(string Email) : IRequest<CustomerProfileDto?>;

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
    int MaxResultCount = 20) : IRequest<PagedResultDto<CustomerListItemDto>>;

#region DTOs

public record CustomerProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageUrl,
    int LoyaltyPoints,
    string LoyaltyTier,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastOrderDate,
    int TotalOrders);

public record DeliveryAddressDto(
    Guid Id,
    string Label,
    string Street,
    string BuildingNumber,
    string? Floor,
    string? Apartment,
    string City,
    string? District,
    string PostalCode,
    string Country,
    double? Latitude,
    double? Longitude,
    string? DeliveryInstructions,
    bool IsDefault,
    string FullAddress);

public record PaymentMethodDto(
    Guid Id,
    string Type,
    string Label,
    string? Last4Digits,
    string? CardBrand,
    bool IsDefault,
    bool IsExpired,
    DateTime? ExpiryDate,
    string DisplayName);

public record LoyaltyInfoDto(
    Guid CustomerId,
    int CurrentPoints,
    int LifetimePoints,
    string CurrentTier,
    string NextTier,
    int PointsToNextTier,
    decimal PointsValue);

public record CustomerListItemDto(
    Guid Id,
    string FullName,
    string Email,
    string LoyaltyTier,
    int LoyaltyPoints,
    bool IsActive,
    DateTime? LastOrderDate,
    int TotalOrders);

public record PagedResultDto<T>(
    List<T> Items,
    int TotalCount);

#endregion
