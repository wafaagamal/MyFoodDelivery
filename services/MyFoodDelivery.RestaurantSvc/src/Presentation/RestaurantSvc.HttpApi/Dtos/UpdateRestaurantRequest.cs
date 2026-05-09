namespace RestaurantSvc.HttpApi.Dtos;

public record UpdateRestaurantRequest(
    string Name,
    string Description,
    string CuisineType,
    string PhoneNumber,
    string Email,
    decimal MinimumOrderAmount,
    decimal DeliveryFee,
    int EstimatedDeliveryMinutes);
