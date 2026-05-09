using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace AuthSvc.HttpApi.Host.DataSeeders;

/// <summary>
/// Seeds initial data: roles, OpenIddict application client, and admin user.
/// </summary>
public class AuthDataSeeder : ITransientDependency
{
    private readonly IdentityRoleManager _roleManager;
    private readonly IdentityUserManager _userManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<AuthDataSeeder> _logger;

    public AuthDataSeeder(
        IdentityRoleManager roleManager,
        IdentityUserManager userManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<AuthDataSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _applicationManager = applicationManager;
        _scopeManager = scopeManager;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        await SeedRolesAsync();
        await SeedScopesAsync();
        await SeedOpenIddictApplicationAsync();
        await SeedAdminUserAsync();
        await uow.CompleteAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roles = new[] { "Customer", "Restaurant", "Delivery", "Admin" };

        foreach (var roleName in roles)
        {
            if (await _roleManager.FindByNameAsync(roleName) != null)
            {
                continue;
            }

            _logger.LogInformation("Creating role: {Role}", roleName);
            var role = new Volo.Abp.Identity.IdentityRole(Guid.NewGuid(), roleName);
            await _roleManager.CreateAsync(role);
        }
    }

    private async Task SeedScopesAsync()
    {
        if (await _scopeManager.FindByNameAsync("myfooddelivery") is not null)
        {
            return;
        }

        _logger.LogInformation("Creating OpenIddict scope: myfooddelivery");

        await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name = "myfooddelivery",
            Resources = { "myfooddelivery-api" }
        });
    }

    private async Task SeedOpenIddictApplicationAsync()
    {
        const string clientId = "angular-customer-portal";

        if (await _applicationManager.FindByClientIdAsync(clientId) is not null)
        {
            return;
        }

        _logger.LogInformation("Creating OpenIddict application: {ClientId}", clientId);

        await _applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = OpenIddictConstants.ClientTypes.Public,
            DisplayName = "MyFoodDelivery Customer Portal",
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.Password,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + "myfooddelivery"
            }
        });
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@myfooddelivery.com";
        const string adminUserName = "admin";

        if (await _userManager.FindByNameAsync(adminUserName) is not null)
        {
            return;
        }

        _logger.LogInformation("Creating default admin user...");

        var adminUser = new Volo.Abp.Identity.IdentityUser(
            Guid.NewGuid(),
            adminUserName,
            adminEmail);

        var result = await _userManager.CreateAsync(adminUser, "Admin@123456!");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create admin user: {Errors}", errors);
            return;
        }

        await _userManager.AddToRoleAsync(adminUser, "Admin");
        _logger.LogInformation("Admin user created successfully.");
    }
}
