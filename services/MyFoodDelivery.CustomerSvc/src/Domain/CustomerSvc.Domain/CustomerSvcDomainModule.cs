using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace CustomerSvc.Domain;

[DependsOn(typeof(AbpDddDomainModule))]
public class CustomerSvcDomainModule : AbpModule
{
}
