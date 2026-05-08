using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace CustomerSvc.HttpApi;

[DependsOn(
    typeof(Application.CustomerSvcApplicationModule),
    typeof(AbpAspNetCoreMvcModule))]
public class CustomerSvcHttpApiModule : AbpModule
{
}
