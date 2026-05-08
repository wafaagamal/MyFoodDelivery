using AutoMapper;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.EventBus.RabbitMq;
using OrderingSvc.Application.Contracts;

namespace OrderingSvc.Application;

[DependsOn(
    typeof(OrderingSvcApplicationContractsModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpEventBusRabbitMqModule)
)]
public class OrderingSvcApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<OrderingSvcApplicationModule>();
        });
    }
}
