using AuthSvc.Domain;
using AuthSvc.Infrastructure.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;

namespace AuthSvc.Infrastructure;

[DependsOn(
    typeof(AuthSvcDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule)
)]
public class AuthSvcInfrastructureModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AuthSvcDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
            options.ReplaceDbContext<IIdentityDbContext>();
            options.ReplaceDbContext<IOpenIddictDbContext>();
            options.ReplaceDbContext<IPermissionManagementDbContext>();
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<AuthSvcDbContext>(c =>
            {
                c.UseSqlServer();
            });
        });
    }
}
