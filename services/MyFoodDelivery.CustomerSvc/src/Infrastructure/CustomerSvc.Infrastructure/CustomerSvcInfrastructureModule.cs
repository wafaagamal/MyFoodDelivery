using CustomerSvc.Domain.Customers;
using CustomerSvc.Infrastructure.EntityFrameworkCore;
using CustomerSvc.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;

namespace CustomerSvc.Infrastructure;

[DependsOn(
    typeof(Domain.CustomerSvcDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpEventBusRabbitMqModule))]
public class CustomerSvcInfrastructureModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<CustomerSvcDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        context.Services.AddAbpDbContext<CustomerSvcReadDbContext>();

        // Register custom repository
        context.Services.AddScoped<ICustomerRepository, EfCustomerRepository>();

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });
    }
}
