using MongoDB.Bson.Serialization;
using RestaurantSvc.Domain.Restaurants;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;
using Volo.Abp.Modularity;

namespace RestaurantSvc.HttpApi.Host;

[DependsOn(typeof(AbpMongoDbModule))]
public class RestaurantSvcMongoDbModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // Configure MongoDB serialization to include private backing fields
        if (!BsonClassMap.IsClassMapRegistered(typeof(Restaurant)))
        {
            BsonClassMap.RegisterClassMap<Restaurant>(cm =>
            {
                cm.AutoMap();
                cm.MapField("_categories").SetElementName("Categories");
                cm.MapField("_menuItems").SetElementName("MenuItems");
                cm.MapField("_openingHours").SetElementName("OpeningHours");
            });
        }
    }
    
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<RestaurantMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<RestaurantMongoDbContext>(includeAllEntities: true);
        });
    }
}
