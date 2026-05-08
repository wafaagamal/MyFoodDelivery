using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace OrderingSvc.Domain;

[DependsOn(typeof(AbpDddDomainModule))]
public class OrderingSvcDomainModule : AbpModule
{
}
