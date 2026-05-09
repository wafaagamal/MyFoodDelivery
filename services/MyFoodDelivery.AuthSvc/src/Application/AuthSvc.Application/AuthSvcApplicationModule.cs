using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;

namespace AuthSvc.Application;

[DependsOn(
    typeof(AbpAccountApplicationModule),
    typeof(AbpIdentityApplicationModule)
)]
public class AuthSvcApplicationModule : AbpModule
{
}
