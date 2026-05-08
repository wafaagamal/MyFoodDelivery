using System;
using Volo.Abp.Domain.Entities;

namespace OrderingSvc.Domain.Orders;

/// <summary>
/// Entity representing an item in an order.
/// </summary>
public class OrderItem : Entity<Guid>
{
    public Guid MenuItemId { get; private set; }
    public string Name { get; private set; } = default!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string? SpecialInstructions { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    private OrderItem() { } // EF Core

    internal OrderItem(
        Guid id,
        Guid menuItemId,
        string name,
        int quantity,
        decimal unitPrice,
        string? specialInstructions)
        : base(id)
    {
        MenuItemId = menuItemId;
        SetName(name);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
        SpecialInstructions = specialInstructions;
    }

    internal void UpdateQuantity(int quantity)
    {
        SetQuantity(quantity);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Item name is required.", nameof(name));
        Name = name;
    }

    private void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(quantity));
        Quantity = quantity;
    }

    private void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        UnitPrice = unitPrice;
    }
}

/// <summary>
/// Value object representing the delivery address for an order.
/// Copied from customer's address at order time (snapshot).
/// </summary>
public class DeliveryAddress
{
    public string Street { get; private set; } = default!;
    public string BuildingNumber { get; private set; } = default!;
    public string? Floor { get; private set; }
    public string? Apartment { get; private set; }
    public string City { get; private set; } = default!;
    public string PostalCode { get; private set; } = default!;
    public string Country { get; private set; } = default!;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? DeliveryInstructions { get; private set; }

    private DeliveryAddress() { } // EF Core

    public DeliveryAddress(
        string street,
        string buildingNumber,
        string? floor,
        string? apartment,
        string city,
        string postalCode,
        string country,
        double latitude,
        double longitude,
        string? deliveryInstructions)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        BuildingNumber = buildingNumber ?? throw new ArgumentNullException(nameof(buildingNumber));
        Floor = floor;
        Apartment = apartment;
        City = city ?? throw new ArgumentNullException(nameof(city));
        PostalCode = postalCode ?? throw new ArgumentNullException(nameof(postalCode));
        Country = country ?? throw new ArgumentNullException(nameof(country));
        Latitude = latitude;
        Longitude = longitude;
        DeliveryInstructions = deliveryInstructions;
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { Street, BuildingNumber };
        
        if (!string.IsNullOrEmpty(Floor))
            parts.Add($"Floor {Floor}");
        
        if (!string.IsNullOrEmpty(Apartment))
            parts.Add($"Apt {Apartment}");
        
        parts.Add(City);
        parts.Add(PostalCode);
        parts.Add(Country);

        return string.Join(", ", parts);
    }
}
