using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace CustomerSvc.Infrastructure.EntityFrameworkCore;

/// <summary>
/// Read-optimized DbContext for Customer service.
/// Uses same connection but configured for read scenarios (no tracking by default).
/// Can be pointed to a read replica in production.
/// </summary>
[ConnectionStringName("CustomerSvcRead")]
public class CustomerSvcReadDbContext : AbpDbContext<CustomerSvcReadDbContext>
{
    public DbSet<CustomerReadModel> Customers { get; set; } = default!;
    public DbSet<AddressReadModel> CustomerAddresses { get; set; } = default!;

    public CustomerSvcReadDbContext(DbContextOptions<CustomerSvcReadDbContext> options)
        : base(options)
    {
        // Disable change tracking for read-only context
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        ChangeTracker.AutoDetectChangesEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomerReadModel>(b =>
        {
            b.ToTable("Customers");
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).HasColumnName("Email");
            b.Property(x => x.PhoneNumber).HasColumnName("PhoneNumber");
            b.Property(x => x.LoyaltyTier).HasConversion<string>();
            
            b.HasMany(x => x.Addresses)
                .WithOne()
                .HasForeignKey(a => a.CustomerId);
        });

        modelBuilder.Entity<AddressReadModel>(b =>
        {
            b.ToTable("CustomerAddresses");
            b.HasKey(x => x.Id);
        });
    }
}

/// <summary>
/// Read model for Customer - flat projection optimized for queries.
/// </summary>
public class CustomerReadModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int LoyaltyPoints { get; set; }
    public int LifetimeLoyaltyPoints { get; set; }
    public Domain.Customers.Events.LoyaltyTier LoyaltyTier { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public int TotalOrders { get; set; }
    public DateTime CreationTime { get; set; }
    
    public List<AddressReadModel> Addresses { get; set; } = new();

    public string FullName => $"{FirstName} {LastName}";
}

/// <summary>
/// Read model for Address - flat projection optimized for queries.
/// </summary>
public class AddressReadModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Label { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string BuildingNumber { get; set; } = default!;
    public string? Floor { get; set; }
    public string? Apartment { get; set; }
    public string City { get; set; } = default!;
    public string? District { get; set; }
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DeliveryInstructions { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}
