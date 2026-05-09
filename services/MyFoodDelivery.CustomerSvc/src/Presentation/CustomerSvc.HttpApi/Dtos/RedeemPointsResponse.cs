namespace CustomerSvc.HttpApi.Dtos;

public record RedeemPointsResponse(
    bool Success,
    string? ErrorMessage);
