using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace CustomerSvc.Application;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(Domain.CustomerSvcDomainModule))]
public class CustomerSvcApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CustomerSvcApplicationModule).Assembly);
        });
    }
}
