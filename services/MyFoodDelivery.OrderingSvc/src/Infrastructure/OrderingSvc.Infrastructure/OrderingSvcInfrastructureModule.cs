using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OrderingSvc.Domain.Services;
using OrderingSvc.Infrastructure.EntityFrameworkCore;
using OrderingSvc.Infrastructure.Services;
using StackExchange.Redis;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.EventBus.RabbitMq;
using Volo.Abp.Modularity;

namespace OrderingSvc.Infrastructure;

[DependsOn(
    typeof(Domain.OrderingSvcDomainModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpEventBusRabbitMqModule))]
public class OrderingSvcInfrastructureModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<OrderingSvcDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });

        var configuration = context.Services.GetConfiguration();
        var redisConnectionString = configuration["Redis:Configuration"] ?? "localhost:6379";
        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;
        context.Services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisOptions));
        context.Services.AddTransient<ICartService, RedisCartService>();
    }
}
