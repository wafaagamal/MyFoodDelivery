using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace RestaurantSvc.Domain;

[DependsOn(
    typeof(AbpDddDomainModule)
)]
public class RestaurantSvcDomainModule : AbpModule
{
}
