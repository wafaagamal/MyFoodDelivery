using System;
using Volo.Abp.Domain.Entities;

namespace CustomerSvc.Domain.Customers;

/// <summary>
/// Entity representing a customer's saved payment method.
/// Owned by the Customer aggregate.
/// </summary>
public class PaymentMethod : Entity<Guid>
{
    public PaymentMethodType Type { get; private set; }
    public string Label { get; private set; } = default!;
    public string? Last4Digits { get; private set; }
    public string? CardBrand { get; private set; }
    public string? ExternalToken { get; private set; } // Token from payment provider
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private PaymentMethod() { } // EF Core

    internal PaymentMethod(
        Guid id,
        PaymentMethodType type,
        string label,
        string? last4Digits,
        string? cardBrand,
        string? externalToken,
        DateTime? expiryDate,
        bool isDefault)
        : base(id)
    {
        Type = type;
        SetLabel(label);
        Last4Digits = last4Digits;
        CardBrand = cardBrand;
        ExternalToken = externalToken;
        ExpiryDate = expiryDate;
        IsDefault = isDefault;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    internal void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    internal void UpdateToken(string externalToken, DateTime? expiryDate)
    {
        ExternalToken = externalToken;
        ExpiryDate = expiryDate;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Payment method label is required.", nameof(label));
        
        if (label.Length > 50)
            throw new ArgumentException("Label cannot exceed 50 characters.", nameof(label));
        
        Label = label.Trim();
    }

    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

    public string GetDisplayName()
    {
        return Type switch
        {
            PaymentMethodType.CreditCard or PaymentMethodType.DebitCard 
                => $"{CardBrand ?? "Card"} •••• {Last4Digits}",
            PaymentMethodType.PayPal 
                => $"PayPal ({Label})",
            PaymentMethodType.ApplePay 
                => "Apple Pay",
            PaymentMethodType.GooglePay 
                => "Google Pay",
            PaymentMethodType.CashOnDelivery 
                => "Cash on Delivery",
            _ => Label
        };
    }
}

public enum PaymentMethodType
{
    CreditCard,
    DebitCard,
    PayPal,
    ApplePay,
    GooglePay,
    CashOnDelivery,
    BankTransfer
}
