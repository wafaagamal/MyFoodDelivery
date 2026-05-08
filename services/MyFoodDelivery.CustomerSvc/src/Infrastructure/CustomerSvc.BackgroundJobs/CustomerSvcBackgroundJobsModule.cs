using Volo.Abp.BackgroundJobs;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;

namespace CustomerSvc.BackgroundJobs;

[DependsOn(
    typeof(Application.CustomerSvcApplicationModule),
    typeof(Infrastructure.CustomerSvcInfrastructureModule),
    typeof(AbpBackgroundJobsModule),
    typeof(AbpEventBusRabbitMqModule))]
public class CustomerSvcBackgroundJobsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure RabbitMQ for event bus with outbox/inbox pattern
        Configure<AbpRabbitMqEventBusOptions>(options =>
        {
            options.ClientName = "CustomerSvc";
            options.ExchangeName = "MyFoodDelivery";
        });
    }
}
