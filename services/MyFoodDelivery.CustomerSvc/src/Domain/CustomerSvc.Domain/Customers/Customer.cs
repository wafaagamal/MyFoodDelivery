using System;
using System.Collections.Generic;
using System.Linq;
using CustomerSvc.Domain.Customers.Events;
using CustomerSvc.Domain.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace CustomerSvc.Domain.Customers;

/// <summary>
/// Customer aggregate root - the main entity representing a customer.
/// Protects all invariants and raises domain events for state changes.
/// </summary>
public class Customer : FullAuditedAggregateRoot<Guid>
{
    private const int MaxAddresses = 10;
    private const int MaxPaymentMethods = 5;
    private const int PointsPerTierLevel = 1000;

    private readonly List<DeliveryAddress> _addresses = new();
    private readonly List<PaymentMethod> _paymentMethods = new();

    public IReadOnlyCollection<DeliveryAddress> Addresses => _addresses.AsReadOnly();
    public IReadOnlyCollection<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public PhoneNumber? Phone { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public int LoyaltyPoints { get; private set; }
    public int LifetimeLoyaltyPoints { get; private set; }
    public LoyaltyTier LoyaltyTier { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastOrderDate { get; private set; }
    public int TotalOrders { get; private set; }

    private Customer() { } // EF Core

    /// <summary>
    /// Creates a new Customer aggregate.
    /// </summary>
    public Customer(
        Guid id,
        string firstName,
        string lastName,
        Email email,
        PhoneNumber? phone = null)
        : base(id)
    {
        SetName(firstName, lastName);
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Phone = phone;
        LoyaltyPoints = 0;
        LifetimeLoyaltyPoints = 0;
        LoyaltyTier = LoyaltyTier.Bronze;
        IsActive = true;
        TotalOrders = 0;

        AddLocalEvent(new CustomerCreatedDomainEvent(
            id, 
            email.Value, 
            firstName, 
            lastName, 
            phone?.Value));
    }

    #region Profile Management

    public void UpdateProfile(string firstName, string lastName, PhoneNumber? phone)
    {
        EnsureActive();
        SetName(firstName, lastName);
        Phone = phone;

        AddLocalEvent(new CustomerProfileUpdatedDomainEvent(
            Id, 
            FirstName, 
            LastName, 
            Phone?.Value));
    }

    public void SetProfileImage(string? imageUrl)
    {
        EnsureActive();
        ProfileImageUrl = imageUrl;
    }

    private void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessException("Customer:FirstNameRequired");
        
        if (firstName.Length > 100)
            throw new BusinessException("Customer:FirstNameTooLong");
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new BusinessException("Customer:LastNameRequired");
        
        if (lastName.Length > 100)
            throw new BusinessException("Customer:LastNameTooLong");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public string GetFullName() => $"{FirstName} {LastName}";

    #endregion

    #region Address Management

    public Guid AddDeliveryAddress(
        string label,
        string street,
        string buildingNumber,
        string? floor,
        string? apartment,
        string city,
        string? district,
        string postalCode,
        string country,
        GeoCoordinate? coordinates = null,
        string? deliveryInstructions = null,
        bool isDefault = false)
    {
        EnsureActive();

        var activeAddresses = _addresses.Count(a => a.IsActive);
        if (activeAddresses >= MaxAddresses)
            throw new BusinessException("Customer:MaxAddressesReached")
                .WithData("MaxAddresses", MaxAddresses);

        var address = new DeliveryAddress(
            Guid.NewGuid(),
            label,
            street,
            buildingNumber,
            floor,
            apartment,
            city,
            district,
            postalCode,
            country,
            coordinates,
            deliveryInstructions,
            isDefault && !_addresses.Any(a => a.IsDefault && a.IsActive));

        // If this should be default and there are existing defaults, clear them
        if (isDefault)
        {
            ClearDefaultAddress();
            address.SetDefault(true);
        }

        // If this is the first address, make it default
        if (!_addresses.Any(a => a.IsActive))
        {
            address.SetDefault(true);
        }

        _addresses.Add(address);

        AddLocalEvent(new CustomerAddressAddedDomainEvent(
            Id,
            address.Id,
            address.Label,
            address.Street,
            address.City,
            address.PostalCode,
            address.Country,
            address.IsDefault));

        return address.Id;
    }

    public void UpdateDeliveryAddress(
        Guid addressId,
        string label,
        string street,
        string buildingNumber,
        string? floor,
        string? apartment,
        string city,
        string? district,
        string postalCode,
        string country,
        GeoCoordinate? coordinates = null,
        string? deliveryInstructions = null)
    {
        EnsureActive();

        var address = GetAddressOrThrow(addressId);
        address.Update(label, street, buildingNumber, floor, apartment, city, district, postalCode, country, coordinates, deliveryInstructions);

        AddLocalEvent(new CustomerAddressUpdatedDomainEvent(
            Id,
            addressId,
            street,
            city,
            postalCode,
            country));
    }

    public void SetDefaultAddress(Guid addressId)
    {
        EnsureActive();

        var address = GetAddressOrThrow(addressId);
        
        if (address.IsDefault)
            return;

        ClearDefaultAddress();
        address.SetDefault(true);
    }

    public void RemoveDeliveryAddress(Guid addressId)
    {
        EnsureActive();

        var address = GetAddressOrThrow(addressId);
        address.Deactivate();

        // If we removed the default, assign a new default
        if (address.IsDefault)
        {
            var newDefault = _addresses.FirstOrDefault(a => a.IsActive && a.Id != addressId);
            newDefault?.SetDefault(true);
        }

        AddLocalEvent(new CustomerAddressRemovedDomainEvent(Id, addressId));
    }

    public DeliveryAddress? GetDefaultAddress()
    {
        return _addresses.FirstOrDefault(a => a.IsDefault && a.IsActive);
    }

    private DeliveryAddress GetAddressOrThrow(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId && a.IsActive);
        if (address == null)
            throw new BusinessException("Customer:AddressNotFound")
                .WithData("AddressId", addressId);
        return address;
    }

    private void ClearDefaultAddress()
    {
        foreach (var addr in _addresses.Where(a => a.IsDefault))
        {
            addr.SetDefault(false);
        }
    }

    #endregion

    #region Payment Method Management

    public Guid AddPaymentMethod(
        PaymentMethodType type,
        string label,
        string? last4Digits = null,
        string? cardBrand = null,
        string? externalToken = null,
        DateTime? expiryDate = null,
        bool isDefault = false)
    {
        EnsureActive();

        var activePaymentMethods = _paymentMethods.Count(p => p.IsActive);
        if (activePaymentMethods >= MaxPaymentMethods)
            throw new BusinessException("Customer:MaxPaymentMethodsReached")
                .WithData("MaxPaymentMethods", MaxPaymentMethods);

        var paymentMethod = new PaymentMethod(
            Guid.NewGuid(),
            type,
            label,
            last4Digits,
            cardBrand,
            externalToken,
            expiryDate,
            isDefault && !_paymentMethods.Any(p => p.IsDefault && p.IsActive));

        if (isDefault)
        {
            ClearDefaultPaymentMethod();
            paymentMethod.SetDefault(true);
        }

        if (!_paymentMethods.Any(p => p.IsActive))
        {
            paymentMethod.SetDefault(true);
        }

        _paymentMethods.Add(paymentMethod);

        AddLocalEvent(new PaymentMethodAddedDomainEvent(
            Id,
            paymentMethod.Id,
            type,
            paymentMethod.IsDefault));

        return paymentMethod.Id;
    }

    public void SetDefaultPaymentMethod(Guid paymentMethodId)
    {
        EnsureActive();

        var paymentMethod = GetPaymentMethodOrThrow(paymentMethodId);
        
        if (paymentMethod.IsDefault)
            return;

        ClearDefaultPaymentMethod();
        paymentMethod.SetDefault(true);
    }

    public void RemovePaymentMethod(Guid paymentMethodId)
    {
        EnsureActive();

        var paymentMethod = GetPaymentMethodOrThrow(paymentMethodId);
        paymentMethod.Deactivate();

        if (paymentMethod.IsDefault)
        {
            var newDefault = _paymentMethods.FirstOrDefault(p => p.IsActive && p.Id != paymentMethodId);
            newDefault?.SetDefault(true);
        }

        AddLocalEvent(new PaymentMethodRemovedDomainEvent(Id, paymentMethodId));
    }

    public PaymentMethod? GetDefaultPaymentMethod()
    {
        return _paymentMethods.FirstOrDefault(p => p.IsDefault && p.IsActive);
    }

    private PaymentMethod GetPaymentMethodOrThrow(Guid paymentMethodId)
    {
        var paymentMethod = _paymentMethods.FirstOrDefault(p => p.Id == paymentMethodId && p.IsActive);
        if (paymentMethod == null)
            throw new BusinessException("Customer:PaymentMethodNotFound")
                .WithData("PaymentMethodId", paymentMethodId);
        return paymentMethod;
    }

    private void ClearDefaultPaymentMethod()
    {
        foreach (var pm in _paymentMethods.Where(p => p.IsDefault))
        {
            pm.SetDefault(false);
        }
    }

    #endregion

    #region Loyalty Points

    public void AddLoyaltyPoints(int points, string reason)
    {
        EnsureActive();

        if (points <= 0)
            throw new BusinessException("Customer:InvalidPointsAmount");

        LoyaltyPoints += points;
        LifetimeLoyaltyPoints += points;

        var previousTier = LoyaltyTier;
        UpdateLoyaltyTier();

        AddLocalEvent(new LoyaltyPointsEarnedDomainEvent(Id, points, LoyaltyPoints, reason));

        if (LoyaltyTier != previousTier)
        {
            AddLocalEvent(new LoyaltyTierChangedDomainEvent(Id, previousTier, LoyaltyTier));
        }
    }

    public bool TryDeductLoyaltyPoints(int points, Guid? orderId = null)
    {
        EnsureActive();

        if (points <= 0)
            throw new BusinessException("Customer:InvalidPointsAmount");

        if (LoyaltyPoints < points)
            return false;

        LoyaltyPoints -= points;

        AddLocalEvent(new LoyaltyPointsSpentDomainEvent(Id, points, LoyaltyPoints, orderId));

        return true;
    }

    public void DeductLoyaltyPoints(int points, Guid? orderId = null)
    {
        if (!TryDeductLoyaltyPoints(points, orderId))
            throw new BusinessException("Customer:InsufficientLoyaltyPoints")
                .WithData("Available", LoyaltyPoints)
                .WithData("Requested", points);
    }

    private void UpdateLoyaltyTier()
    {
        LoyaltyTier = LifetimeLoyaltyPoints switch
        {
            >= 10000 => LoyaltyTier.Platinum,
            >= 5000 => LoyaltyTier.Gold,
            >= 2000 => LoyaltyTier.Silver,
            _ => LoyaltyTier.Bronze
        };
    }

    public int GetPointsToNextTier()
    {
        return LoyaltyTier switch
        {
            LoyaltyTier.Bronze => 2000 - LifetimeLoyaltyPoints,
            LoyaltyTier.Silver => 5000 - LifetimeLoyaltyPoints,
            LoyaltyTier.Gold => 10000 - LifetimeLoyaltyPoints,
            LoyaltyTier.Platinum => 0,
            _ => 0
        };
    }

    #endregion

    #region Order Tracking

    public void RecordOrderPlaced()
    {
        TotalOrders++;
        LastOrderDate = DateTime.UtcNow;
    }

    #endregion

    #region Account Status

    public void Deactivate(string reason)
    {
        if (!IsActive)
            throw new BusinessException("Customer:AlreadyDeactivated");

        IsActive = false;
        AddLocalEvent(new CustomerDeactivatedDomainEvent(Id, reason));
    }

    public void Reactivate()
    {
        if (IsActive)
            throw new BusinessException("Customer:AlreadyActive");

        IsActive = true;
        AddLocalEvent(new CustomerReactivatedDomainEvent(Id));
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new BusinessException("Customer:AccountDeactivated");
    }

    #endregion
}
