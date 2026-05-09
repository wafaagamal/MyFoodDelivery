using System;

namespace CustomerSvc.Application.Contracts.Customers.Dtos;

public record LoyaltyInfoDto(
    Guid CustomerId,
    int CurrentPoints,
    int LifetimePoints,
    string CurrentTier,
    string NextTier,
    int PointsToNextTier,
    decimal PointsValue);
