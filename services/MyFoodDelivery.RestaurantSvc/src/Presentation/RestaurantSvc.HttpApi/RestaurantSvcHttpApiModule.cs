using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using RestaurantSvc.Application;

namespace RestaurantSvc.HttpApi;

[DependsOn(
    typeof(RestaurantSvcApplicationModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class RestaurantSvcHttpApiModule : AbpModule
{
}
