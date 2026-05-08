using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;

namespace RestaurantSvc.Application;

[DependsOn(
    typeof(AbpObjectExtendingModule),
    typeof(AbpDddApplicationContractsModule)
)]
public class RestaurantSvcApplicationContractsModule : AbpModule
{
}
