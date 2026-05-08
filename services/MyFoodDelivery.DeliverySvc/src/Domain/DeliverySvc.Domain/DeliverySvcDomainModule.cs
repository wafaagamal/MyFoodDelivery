using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace DeliverySvc.Domain;

[DependsOn(typeof(AbpDddDomainModule))]
public class DeliverySvcDomainModule : AbpModule
{
}
