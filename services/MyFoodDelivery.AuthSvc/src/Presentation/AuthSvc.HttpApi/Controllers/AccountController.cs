using AuthSvc.Application.Contracts.Account.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyFoodDelivery.Shared.Events;
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

    public AccountController(
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        IDistributedEventBus eventBus)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _eventBus = eventBus;
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

        await _eventBus.PublishAsync(new UserCreatedEto(
            UserId:      user.Id,
            Email:       request.Email,
            FirstName:   request.FirstName,
            LastName:    request.LastName,
            PhoneNumber: string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber,
            Role:        userRole));

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
}
