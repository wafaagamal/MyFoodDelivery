namespace AuthSvc.Application.Contracts.Account.Dtos;

public sealed class RegisterResultDto
{
    public string Message { get; init; } = string.Empty;
    public System.Guid UserId { get; init; }
}
