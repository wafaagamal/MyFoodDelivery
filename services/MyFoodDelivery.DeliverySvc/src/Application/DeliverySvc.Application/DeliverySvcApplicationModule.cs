using DeliverySvc.Application.Contracts;
using DeliverySvc.Infrastructure;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;

namespace DeliverySvc.Application;

[DependsOn(
    typeof(DeliverySvcApplicationContractsModule),
    typeof(DeliverySvcInfrastructureModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpEventBusRabbitMqModule))]
public class DeliverySvcApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<DeliverySvcApplicationModule>();
        });
    }
}
