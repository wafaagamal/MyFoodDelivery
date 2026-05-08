using CustomerSvc.Domain.Customers;
using CustomerSvc.Infrastructure.EntityFrameworkCore.Configurations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace CustomerSvc.Infrastructure.EntityFrameworkCore;

/// <summary>
/// Write-side DbContext for Customer service.
/// Contains the full aggregate with all entities.
/// </summary>
[ConnectionStringName("CustomerSvcWrite")]
public class CustomerSvcDbContext : AbpDbContext<CustomerSvcDbContext>
{
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<DeliveryAddress> CustomerAddresses { get; set; } = default!;
    public DbSet<PaymentMethod> CustomerPaymentMethods { get; set; } = default!;

    public CustomerSvcDbContext(DbContextOptions<CustomerSvcDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new DeliveryAddressConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentMethodConfiguration());
    }
}
