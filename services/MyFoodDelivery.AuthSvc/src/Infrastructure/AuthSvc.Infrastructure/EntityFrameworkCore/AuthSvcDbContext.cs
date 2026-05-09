using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Authorizations;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.OpenIddict.Tokens;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;

namespace AuthSvc.Infrastructure.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class AuthSvcDbContext :
    AbpDbContext<AuthSvcDbContext>,
    IIdentityDbContext,
    IOpenIddictDbContext,
    IPermissionManagementDbContext
{
    // ABP Identity tables
    public DbSet<IdentityUser> Users { get; set; } = default!;
    public DbSet<IdentityRole> Roles { get; set; } = default!;
    public DbSet<IdentityClaimType> ClaimTypes { get; set; } = default!;
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; } = default!;
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; } = default!;
    public DbSet<IdentityLinkUser> LinkUsers { get; set; } = default!;
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; } = default!;
    public DbSet<IdentitySession> Sessions { get; set; } = default!;

    // Permission management tables
    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; } = default!;
    public DbSet<PermissionDefinitionRecord> Permissions { get; set; } = default!;
    public DbSet<PermissionGrant> PermissionGrants { get; set; } = default!;

    // OpenIddict tables
    public DbSet<OpenIddictApplication> Applications { get; set; } = default!;
    public DbSet<OpenIddictAuthorization> Authorizations { get; set; } = default!;
    public DbSet<OpenIddictScope> Scopes { get; set; } = default!;
    public DbSet<OpenIddictToken> Tokens { get; set; } = default!;

    public AuthSvcDbContext(DbContextOptions<AuthSvcDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureIdentity();
        modelBuilder.ConfigureOpenIddict();
        modelBuilder.ConfigurePermissionManagement();
    }
}

