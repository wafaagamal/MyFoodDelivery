using System;

namespace CustomerSvc.Application.Contracts.Customers.Dtos;

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
