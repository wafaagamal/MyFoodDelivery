using System;
using CustomerSvc.Domain.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace CustomerSvc.Domain.Customers;

/// <summary>
/// Entity representing a customer's delivery address.
/// Owned by the Customer aggregate.
/// </summary>
public class DeliveryAddress : Entity<Guid>
{
    public string Label { get; private set; } = default!;
    public string Street { get; private set; } = default!;
    public string BuildingNumber { get; private set; } = default!;
    public string? Floor { get; private set; }
    public string? Apartment { get; private set; }
    public string City { get; private set; } = default!;
    public string? District { get; private set; }
    public string PostalCode { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public GeoCoordinate? Coordinates { get; private set; }
    public string? DeliveryInstructions { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private DeliveryAddress() { } // EF Core

    internal DeliveryAddress(
        Guid id,
        string label,
        string street,
        string buildingNumber,
        string? floor,
        string? apartment,
        string city,
        string? district,
        string postalCode,
        string country,
        GeoCoordinate? coordinates,
        string? deliveryInstructions,
        bool isDefault)
        : base(id)
    {
        SetLabel(label);
        SetStreet(street);
        BuildingNumber = buildingNumber ?? throw new ArgumentNullException(nameof(buildingNumber));
        Floor = floor;
        Apartment = apartment;
        SetCity(city);
        District = district;
        SetPostalCode(postalCode);
        SetCountry(country);
        Coordinates = coordinates;
        DeliveryInstructions = deliveryInstructions;
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

    internal void Update(
        string label,
        string street,
        string buildingNumber,
        string? floor,
        string? apartment,
        string city,
        string? district,
        string postalCode,
        string country,
        GeoCoordinate? coordinates,
        string? deliveryInstructions)
    {
        SetLabel(label);
        SetStreet(street);
        BuildingNumber = buildingNumber ?? throw new ArgumentNullException(nameof(buildingNumber));
        Floor = floor;
        Apartment = apartment;
        SetCity(city);
        District = district;
        SetPostalCode(postalCode);
        SetCountry(country);
        Coordinates = coordinates;
        DeliveryInstructions = deliveryInstructions;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetLabel(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Address label is required.", nameof(label));
        
        if (label.Length > 50)
            throw new ArgumentException("Address label cannot exceed 50 characters.", nameof(label));
        
        Label = label.Trim();
    }

    private void SetStreet(string street)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required.", nameof(street));
        
        if (street.Length > 200)
            throw new ArgumentException("Street cannot exceed 200 characters.", nameof(street));
        
        Street = street.Trim();
    }

    private void SetCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));
        
        if (city.Length > 100)
            throw new ArgumentException("City cannot exceed 100 characters.", nameof(city));
        
        City = city.Trim();
    }

    private void SetPostalCode(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required.", nameof(postalCode));
        
        if (postalCode.Length > 20)
            throw new ArgumentException("Postal code cannot exceed 20 characters.", nameof(postalCode));
        
        PostalCode = postalCode.Trim();
    }

    private void SetCountry(string country)
    {
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required.", nameof(country));
        
        if (country.Length > 100)
            throw new ArgumentException("Country cannot exceed 100 characters.", nameof(country));
        
        Country = country.Trim();
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { Street, BuildingNumber };
        
        if (!string.IsNullOrEmpty(Floor))
            parts.Add($"Floor {Floor}");
        
        if (!string.IsNullOrEmpty(Apartment))
            parts.Add($"Apt {Apartment}");
        
        parts.Add(City);
        
        if (!string.IsNullOrEmpty(District))
            parts.Add(District);
        
        parts.Add(PostalCode);
        parts.Add(Country);

        return string.Join(", ", parts);
    }
}
