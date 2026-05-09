using System;

namespace CustomerSvc.Application.Contracts.Customers.Dtos;

public record CustomerProfileDto(
    Guid Id,
    int LoyaltyPoints,
    string LoyaltyTier,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastOrderDate,
    int TotalOrders,
    string? PhoneNumber = null,
    string? ProfileImageUrl = null);
