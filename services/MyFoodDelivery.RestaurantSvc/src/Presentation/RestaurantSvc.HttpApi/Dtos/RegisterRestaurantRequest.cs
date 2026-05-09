namespace RestaurantSvc.HttpApi.Dtos;

public record RegisterRestaurantRequest(
    string Name,
    string Description,
    string CuisineType,
    string PhoneNumber,
    string Email,
    string Street,
    string BuildingNumber,
    string City,
    string? District,
    string PostalCode,
    string Country,
    double Latitude,
    double Longitude,
    decimal MinimumOrderAmount,
    decimal DeliveryFee,
    int EstimatedDeliveryMinutes);
