using DeliverySvc.Domain;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace DeliverySvc.Application.Contracts;

[DependsOn(
    typeof(DeliverySvcDomainModule),
    typeof(AbpDddApplicationContractsModule))]
public class DeliverySvcApplicationContractsModule : AbpModule
{
}
