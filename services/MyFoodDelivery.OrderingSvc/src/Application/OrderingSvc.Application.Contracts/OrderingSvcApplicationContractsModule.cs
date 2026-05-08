using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace OrderingSvc.Application.Contracts;

[DependsOn(
    typeof(AbpAuthorizationModule),
    typeof(AbpDddApplicationContractsModule)
)]
public class OrderingSvcApplicationContractsModule : AbpModule
{
}
