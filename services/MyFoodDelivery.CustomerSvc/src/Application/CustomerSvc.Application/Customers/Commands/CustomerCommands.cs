using System;
using MediatR;

namespace CustomerSvc.Application.Customers.Commands;

/// <summary>
/// Command to register a new customer (typically from IdentityServer user creation).
/// </summary>
public record RegisterCustomerCommand(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber = null) : IRequest<Guid>;

/// <summary>
/// Command to update a customer's profile.
/// </summary>
public record UpdateCustomerProfileCommand(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? ProfileImageUrl = null) : IRequest;

/// <summary>
/// Command to add a delivery address to a customer.
/// </summary>
public record AddDeliveryAddressCommand(
    Guid CustomerId,
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
    bool IsDefault = false) : IRequest<Guid>;

/// <summary>
/// Command to update an existing delivery address.
/// </summary>
public record UpdateDeliveryAddressCommand(
    Guid CustomerId,
    Guid AddressId,
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
    string? DeliveryInstructions) : IRequest;

/// <summary>
/// Command to remove a delivery address.
/// </summary>
public record RemoveDeliveryAddressCommand(
    Guid CustomerId,
    Guid AddressId) : IRequest;

/// <summary>
/// Command to set a delivery address as default.
/// </summary>
public record SetDefaultAddressCommand(
    Guid CustomerId,
    Guid AddressId) : IRequest;

/// <summary>
/// Command to add a payment method.
/// </summary>
public record AddPaymentMethodCommand(
    Guid CustomerId,
    string Type,
    string Label,
    string? Last4Digits,
    string? CardBrand,
    string? ExternalToken,
    DateTime? ExpiryDate,
    bool IsDefault = false) : IRequest<Guid>;

/// <summary>
/// Command to remove a payment method.
/// </summary>
public record RemovePaymentMethodCommand(
    Guid CustomerId,
    Guid PaymentMethodId) : IRequest;

/// <summary>
/// Command to set a payment method as default.
/// </summary>
public record SetDefaultPaymentMethodCommand(
    Guid CustomerId,
    Guid PaymentMethodId) : IRequest;

/// <summary>
/// Command to add loyalty points to a customer.
/// </summary>
public record AddLoyaltyPointsCommand(
    Guid CustomerId,
    int Points,
    string Reason) : IRequest;

/// <summary>
/// Command to deduct (redeem) loyalty points.
/// </summary>
public record DeductLoyaltyPointsCommand(
    Guid CustomerId,
    int Points,
    Guid? OrderId = null) : IRequest<bool>;

/// <summary>
/// Command to deactivate a customer account.
/// </summary>
public record DeactivateCustomerCommand(
    Guid CustomerId,
    string Reason) : IRequest;

/// <summary>
/// Command to reactivate a customer account.
/// </summary>
public record ReactivateCustomerCommand(
    Guid CustomerId) : IRequest;
