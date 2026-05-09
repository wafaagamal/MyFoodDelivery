using AuthSvc.Domain.Shared;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;

namespace AuthSvc.Domain;

[DependsOn(
    typeof(AbpIdentityDomainModule),
    typeof(AbpOpenIddictDomainModule),
    typeof(AuthSvcDomainSharedModule)
)]
public class AuthSvcDomainModule : AbpModule
{
}
