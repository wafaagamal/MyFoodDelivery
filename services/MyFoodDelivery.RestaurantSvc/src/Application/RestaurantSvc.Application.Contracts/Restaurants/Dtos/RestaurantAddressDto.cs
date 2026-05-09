using System;

namespace RestaurantSvc.Application.Contracts.Restaurants.Dtos;

public record RestaurantAddressDto(
    string Street,
    string BuildingNumber,
    string City,
    string? District,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude,
    string FullAddress);
