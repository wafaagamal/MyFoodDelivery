namespace CustomerSvc.HttpApi.Dtos;

public record AddAddressRequest(
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
    bool IsDefault = false);
