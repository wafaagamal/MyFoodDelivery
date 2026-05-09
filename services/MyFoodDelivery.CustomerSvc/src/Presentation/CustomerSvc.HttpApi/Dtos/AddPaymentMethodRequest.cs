using System;

namespace CustomerSvc.HttpApi.Dtos;

public record AddPaymentMethodRequest(
    string Type,
    string Label,
    string? Last4Digits,
    string? CardBrand,
    string? ExternalToken,
    DateTime? ExpiryDate,
    bool IsDefault = false);
