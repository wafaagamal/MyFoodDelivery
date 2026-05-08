using DeliverySvc.Domain;
using DeliverySvc.Infrastructure.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;

namespace DeliverySvc.Infrastructure;

[DependsOn(
    typeof(DeliverySvcDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpEventBusRabbitMqModule))]
public class DeliverySvcInfrastructureModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<DeliverySvcDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });
    }
}
