namespace OrderingSvc.Application.Contracts.Orders.Dtos;

public class DeliveryAddressDto
{
    public string Street { get; set; } = default!;
    public string BuildingNumber { get; set; } = default!;
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public string? Landmark { get; set; }
    public string? DeliveryInstructions { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

