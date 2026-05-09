using System;

namespace MyFoodDelivery.Shared.Events;

/// <summary>
/// Published when a new customer registers in the system.
/// Identity data (name/email) is already in UserCreatedEto from AuthSvc.
/// </summary>
public record CustomerRegisteredEto(
    Guid CustomerId);

/// <summary>
/// Published when a customer adds a new delivery address.
/// May be consumed by OrderingSvc for address validation/caching.
/// </summary>
public record CustomerAddressAddedEto(
    Guid CustomerId,
    Guid AddressId,
    string Street,
    string City,
    string PostalCode,
    string Country,
    bool IsDefault);

/// <summary>
/// Published when a customer's loyalty points balance changes.
/// May be consumed by analytics or notification services.
/// </summary>
public record LoyaltyPointsUpdatedEto(
    Guid CustomerId,
    int PointsChanged,
    int NewTotal,
    LoyaltyPointsChangeReason Reason);

/// <summary>
/// Published when a customer updates their profile in AuthSvc.
/// </summary>
public record CustomerProfileUpdatedEto(
    Guid CustomerId);

/// <summary>
/// Published when a customer deactivates their account.
/// </summary>
public record CustomerDeactivatedEto(
    Guid CustomerId,
    DateTime DeactivatedAt);

/// <summary>
/// Reason for loyalty points change.
/// </summary>
public enum LoyaltyPointsChangeReason
{
    OrderCompleted,
    Referral,
    Promotion,
    Redemption,
    Adjustment
}

/// <summary>
/// Published by IdentityServer when a new user is created.
/// Consumed by CustomerSvc to initialize the Customer aggregate.
/// </summary>
public record UserCreatedEto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserRole Role);

public enum UserRole
{
    Customer,
    RestaurantOwner,
    Rider,
    Admin
}
