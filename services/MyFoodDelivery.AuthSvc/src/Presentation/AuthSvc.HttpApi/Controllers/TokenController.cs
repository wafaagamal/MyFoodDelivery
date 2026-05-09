using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using Volo.Abp.Identity;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthSvc.HttpApi.Controllers;

/// <summary>
/// Handles the OpenIddict token endpoint for password grant and refresh token grant.
/// Validates credentials using ABP IdentityUserManager and issues JWT access tokens.
/// </summary>
[ApiController]
[IgnoreAntiforgeryToken]
public class TokenController : ControllerBase
{
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;
    private readonly IOpenIddictApplicationManager _applicationManager;

    public TokenController(
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        IOpenIddictApplicationManager applicationManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _applicationManager = applicationManager;
    }

    [HttpPost("~/connect/token")]
    [Consumes("application/x-www-form-urlencoded")]
    [Produces("application/json")]
    public async Task<IActionResult> ExchangeAsync()
    {
        var feature = HttpContext.Features.Get<OpenIddictServerAspNetCoreFeature>()
            ?? throw new InvalidOperationException("The OpenIddict server feature cannot be retrieved.");

        var request = feature.Transaction.Request
            ?? throw new InvalidOperationException("The OpenIddict server request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            return await HandlePasswordGrantAsync(request);
        }

        if (request.IsRefreshTokenGrantType())
        {
            return await HandleRefreshTokenGrantAsync();
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = Errors.UnsupportedGrantType,
            ErrorDescription = "The specified grant type is not supported."
        });
    }

    private async Task<IActionResult> HandlePasswordGrantAsync(OpenIddictRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username!)
                   ?? await _userManager.FindByEmailAsync(request.Username!);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password!))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The username or password is incorrect."
                }));
        }

        var principal = await BuildClaimsPrincipalAsync(user, request.GetScopes());
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> HandleRefreshTokenGrantAsync()
    {
        var result = await HttpContext.AuthenticateAsync(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var userId = result.Principal?.GetClaim(Claims.Subject);
        if (userId is null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The refresh token is no longer valid."
                }));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The user account no longer exists."
                }));
        }

        var scopes = result.Principal?.GetScopes() ?? Enumerable.Empty<string>();
        var principal = await BuildClaimsPrincipalAsync(user, scopes);
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<ClaimsPrincipal> BuildClaimsPrincipalAsync(
        Volo.Abp.Identity.IdentityUser user,
        IEnumerable<string> scopes)
    {
        var identity = new ClaimsIdentity(
            authenticationType: "OpenIddict",
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.AddClaim(Claims.Subject, user.Id.ToString());
        identity.AddClaim(Claims.Name, user.UserName ?? user.Email!);
        identity.AddClaim(Claims.Email, user.Email!);

        if (!string.IsNullOrWhiteSpace(user.Name))
        {
            identity.AddClaim(Claims.GivenName, user.Name);
        }

        if (!string.IsNullOrWhiteSpace(user.Surname))
        {
            identity.AddClaim(Claims.FamilyName, user.Surname);
        }

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            identity.AddClaim(Claims.Role, role);
        }

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopes);

        foreach (var claim in principal.Claims)
        {
            claim.SetDestinations(GetClaimDestinations(claim, principal));
        }

        return principal;
    }

    private static IEnumerable<string> GetClaimDestinations(Claim claim, ClaimsPrincipal principal)
    {
        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;
                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (principal.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
