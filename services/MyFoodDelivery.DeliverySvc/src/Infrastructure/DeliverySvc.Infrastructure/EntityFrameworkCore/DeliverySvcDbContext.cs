using DeliverySvc.Domain.DeliveryTasks;
using DeliverySvc.Domain.Riders;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace DeliverySvc.Infrastructure.EntityFrameworkCore;

[ConnectionStringName("Default")]
public class DeliverySvcDbContext : AbpDbContext<DeliverySvcDbContext>
{
    public DbSet<Rider> Riders { get; set; } = default!;
    public DbSet<DeliveryTask> DeliveryTasks { get; set; } = default!;

    public DeliverySvcDbContext(DbContextOptions<DeliverySvcDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureDeliverySvc();
    }
}
