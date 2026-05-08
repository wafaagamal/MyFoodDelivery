using MongoDB.Driver;
using RestaurantSvc.Domain.Restaurants;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace RestaurantSvc.HttpApi.Host;

[ConnectionStringName("MongoDB")]
public class RestaurantMongoDbContext : AbpMongoDbContext
{
    public IMongoCollection<Restaurant> Restaurants => Collection<Restaurant>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        modelBuilder.Entity<Restaurant>(b =>
        {
            b.CollectionName = "restaurants";
        });
    }
}
