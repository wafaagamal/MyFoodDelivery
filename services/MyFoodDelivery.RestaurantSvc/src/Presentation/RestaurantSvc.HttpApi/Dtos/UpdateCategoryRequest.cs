namespace RestaurantSvc.HttpApi.Dtos;

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder);
