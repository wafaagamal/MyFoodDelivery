using AuthSvc.Application.Contracts.Account.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MyFoodDelivery.Shared.Events;
using System.Text;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.Identity;

namespace AuthSvc.HttpApi.Controllers;

/// <summary>
/// Handles customer self-registration. 
/// Follows ABP conventions: thin controller, logic delegated to managers.
/// </summary>
[ApiController]
[Route("api/account")]
[AllowAnonymous]
public class AccountController : ControllerBase
{
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;
    private readonly IDistributedEventBus _eventBus;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        IDistributedEventBus eventBus,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _eventBus = eventBus;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user and publishes UserCreatedEto so downstream services
    /// (CustomerSvc, DeliverySvc) can create the corresponding profile.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequestDto request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Conflict(new { message = "A user with this email already exists." });
        }

        var user = new Volo.Abp.Identity.IdentityUser(
            Guid.NewGuid(),
            userName: request.Email,
            email: request.Email);

        user.Name = request.FirstName;
        user.Surname = request.LastName;

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "Registration failed.",
                errors = createResult.Errors.Select(e => e.Description)
            });
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            await _userManager.SetPhoneNumberAsync(user, request.PhoneNumber);
        }

        var roleName = NormalizeRole(request.Role);
        if (await _roleManager.FindByNameAsync(roleName) is not null)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        var userRole = request.Role?.ToLowerInvariant() switch
        {
            "delivery" or "rider" => UserRole.Rider,
            "restaurant"          => UserRole.RestaurantOwner,
            "admin"               => UserRole.Admin,
            _                     => UserRole.Customer
        };

        // Publish outside the Unit of Work (onUnitOfWorkComplete: false) so that
        // a missing message broker (RabbitMQ) does not roll back the user creation.
        try
        {
            await _eventBus.PublishAsync(new UserCreatedEto(
                UserId:      user.Id,
                Email:       request.Email,
                FirstName:   request.FirstName,
                LastName:    request.LastName,
                PhoneNumber: string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber,
                Role:        userRole),
                onUnitOfWorkComplete: false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "UserCreatedEto could not be published for user {UserId}. " +
                "Message broker may be unavailable. Registration still succeeded.", user.Id);
        }

        return Ok(new { message = "Registration successful. You can now log in.", userId = user.Id });
    }

    private static string NormalizeRole(string? role) =>
        role?.ToLowerInvariant() switch
        {
            "restaurant" => "Restaurant",
            "delivery"   => "Delivery",
            "admin"      => "Admin",
            _            => "Customer"
        };

    /// <summary>
    /// Initiates a password reset. In this dev environment the reset token is
    /// returned directly in the response body (no email service). In production
    /// you would send the token via email and return only a generic 200.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email))
            return BadRequest(new { message = "Email is required." });

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Do not reveal whether the user exists
            return Ok(new { message = "If that email is registered you will receive a reset code shortly." });
        }

        var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Base64Url-encode so it is safe to transmit in JSON / URL
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

        // DEV-ONLY: return token in response so the UI can complete the flow without email.
        return Ok(new
        {
            message   = "Reset code generated. (Dev mode — use the token below.)",
            devToken  = encodedToken,
            userId    = user.Id.ToString()
        });
    }

    /// <summary>Completes a password reset using the token from ForgotPassword.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request?.Email) ||
            string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "Email, token and new password are required." });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid request." });

        string rawToken;
        try
        {
            rawToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch
        {
            return BadRequest(new { message = "Invalid or malformed reset token." });
        }

        var result = await _userManager.ResetPasswordAsync(user, rawToken, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Password reset failed.",
                errors  = result.Errors.Select(e => e.Description)
            });
        }

        return Ok(new { message = "Password has been reset successfully. You can now log in." });
    }
}
