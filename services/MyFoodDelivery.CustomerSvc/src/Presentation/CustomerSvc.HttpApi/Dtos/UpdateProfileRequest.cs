namespace CustomerSvc.HttpApi.Dtos;

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? ProfileImageUrl);
