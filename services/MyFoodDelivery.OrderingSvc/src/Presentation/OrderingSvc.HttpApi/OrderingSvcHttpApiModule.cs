using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using OrderingSvc.Application;

namespace OrderingSvc.HttpApi;

[DependsOn(
    typeof(OrderingSvcApplicationModule),
    typeof(AbpAspNetCoreMvcModule)
)]
public class OrderingSvcHttpApiModule : AbpModule
{
}
