using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace AuthSvc.HttpApi;

[DependsOn(typeof(AbpAspNetCoreMvcModule))]
public class AuthSvcHttpApiModule : AbpModule
{
}
