namespace RestaurantSvc.HttpApi.Dtos;

public record AddCategoryRequest(
    string Name,
    string? Description,
    int DisplayOrder);
