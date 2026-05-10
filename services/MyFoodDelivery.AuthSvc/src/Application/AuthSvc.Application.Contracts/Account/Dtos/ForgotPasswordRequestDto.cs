namespace AuthSvc.Application.Contracts.Account.Dtos;

public sealed class ForgotPasswordRequestDto
{
    public string Email { get; init; } = string.Empty;
}

public sealed class ResetPasswordRequestDto
{
    public string Email       { get; init; } = string.Empty;
    public string Token       { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
