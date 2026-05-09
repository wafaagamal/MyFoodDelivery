using System;

namespace CustomerSvc.Application.Contracts.Customers.Dtos;

public record CustomerListItemDto(
    Guid Id,
    string LoyaltyTier,
    int LoyaltyPoints,
    bool IsActive,
    DateTime? LastOrderDate,
    int TotalOrders);
