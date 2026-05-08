using System;
using Volo.Abp;

namespace CustomerSvc.Domain.Customers.Events;

/// <summary>
/// Base class for all Customer domain events.
/// </summary>
public abstract record CustomerDomainEvent
{
    public Guid CustomerId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when a new customer is created.
/// </summary>
public record CustomerCreatedDomainEvent(
    Guid CustomerId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer updates their profile.
/// </summary>
public record CustomerProfileUpdatedDomainEvent(
    Guid CustomerId,
    string FirstName,
    string LastName,
    string? PhoneNumber) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer adds a delivery address.
/// </summary>
public record CustomerAddressAddedDomainEvent(
    Guid CustomerId,
    Guid AddressId,
    string Label,
    string Street,
    string City,
    string PostalCode,
    string Country,
    bool IsDefault) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer updates a delivery address.
/// </summary>
public record CustomerAddressUpdatedDomainEvent(
    Guid CustomerId,
    Guid AddressId,
    string Street,
    string City,
    string PostalCode,
    string Country) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer removes a delivery address.
/// </summary>
public record CustomerAddressRemovedDomainEvent(
    Guid CustomerId,
    Guid AddressId) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer adds a payment method.
/// </summary>
public record PaymentMethodAddedDomainEvent(
    Guid CustomerId,
    Guid PaymentMethodId,
    PaymentMethodType Type,
    bool IsDefault) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer removes a payment method.
/// </summary>
public record PaymentMethodRemovedDomainEvent(
    Guid CustomerId,
    Guid PaymentMethodId) : CustomerDomainEvent;

/// <summary>
/// Raised when loyalty points are earned.
/// </summary>
public record LoyaltyPointsEarnedDomainEvent(
    Guid CustomerId,
    int PointsEarned,
    int NewTotal,
    string Reason) : CustomerDomainEvent;

/// <summary>
/// Raised when loyalty points are spent/redeemed.
/// </summary>
public record LoyaltyPointsSpentDomainEvent(
    Guid CustomerId,
    int PointsSpent,
    int NewTotal,
    Guid? OrderId) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer's account is deactivated.
/// </summary>
public record CustomerDeactivatedDomainEvent(
    Guid CustomerId,
    string Reason) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer's account is reactivated.
/// </summary>
public record CustomerReactivatedDomainEvent(
    Guid CustomerId) : CustomerDomainEvent;

/// <summary>
/// Raised when a customer's loyalty tier changes.
/// </summary>
public record LoyaltyTierChangedDomainEvent(
    Guid CustomerId,
    LoyaltyTier PreviousTier,
    LoyaltyTier NewTier) : CustomerDomainEvent;

public enum LoyaltyTier
{
    Bronze,
    Silver,
    Gold,
    Platinum
}
