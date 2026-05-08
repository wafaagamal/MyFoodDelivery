using Microsoft.EntityFrameworkCore;
using OrderingSvc.Domain.Orders;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace OrderingSvc.Infrastructure.EntityFrameworkCore;

/// <summary>
/// DbContext for OrderingSvc - manages Orders aggregate.
/// </summary>
[ConnectionStringName("Default")]
public class OrderingSvcDbContext : AbpDbContext<OrderingSvcDbContext>
{
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderItem> OrderItems { get; set; } = default!;

    public OrderingSvcDbContext(DbContextOptions<OrderingSvcDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderingSvcDbContext).Assembly);
    }
}
