using Microsoft.Extensions.DependencyInjection;
using RestaurantSvc.Domain;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace RestaurantSvc.Application;

[DependsOn(
    typeof(RestaurantSvcDomainModule),
    typeof(RestaurantSvcApplicationContractsModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpAuthorizationModule)
)]
public class RestaurantSvcApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<RestaurantSvcApplicationModule>();
        });

        // Register MediatR with handlers from this assembly
        context.Services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(RestaurantSvcApplicationModule).Assembly));
    }
}
