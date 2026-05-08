using DeliverySvc.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace DeliverySvc.HttpApi;

[DependsOn(
    typeof(DeliverySvcApplicationModule),
    typeof(AbpAspNetCoreMvcModule))]
public class DeliverySvcHttpApiModule : AbpModule
{
}
