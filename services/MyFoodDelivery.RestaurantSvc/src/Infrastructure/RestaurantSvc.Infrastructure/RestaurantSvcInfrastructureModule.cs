using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace RestaurantSvc.Infrastructure;

[DependsOn(typeof(AbpMongoDbModule))]
public class RestaurantSvcInfrastructureModule : AbpModule
{
}
