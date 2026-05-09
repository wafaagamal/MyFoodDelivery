using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;

namespace AuthSvc.Domain.Shared;

[DependsOn(
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule)
)]
public class AuthSvcDomainSharedModule : AbpModule
{
}
