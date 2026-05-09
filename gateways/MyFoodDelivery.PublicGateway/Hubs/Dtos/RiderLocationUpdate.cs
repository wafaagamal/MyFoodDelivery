namespace MyFoodDelivery.PublicGateway.Hubs.Dtos;

/// <summary>
/// DTO for rider location updates sent via SignalR.
/// </summary>
public record RiderLocationUpdate(
    double Latitude,
    double Longitude,
    double? Heading,
    double? Speed,
    System.Guid? CurrentOrderId,
    int? EstimatedArrivalMinutes);
