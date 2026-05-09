using System;

namespace CustomerSvc.Application.Contracts.Customers.Dtos;

public record DeliveryAddressDto(
    Guid Id,
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
    bool IsDefault,
    string FullAddress);
