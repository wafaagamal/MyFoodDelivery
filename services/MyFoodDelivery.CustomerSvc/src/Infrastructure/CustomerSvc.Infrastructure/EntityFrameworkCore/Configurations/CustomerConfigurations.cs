using CustomerSvc.Domain.Customers;
using CustomerSvc.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CustomerSvc.Infrastructure.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core configuration for Customer aggregate root.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.ConfigureByConvention();

        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Value object: Email
        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .IsRequired()
                .HasMaxLength(254);

            email.HasIndex(e => e.Value)
                .IsUnique()
                .HasDatabaseName("IX_Customers_Email");
        });

        // Value object: PhoneNumber (optional)
        builder.OwnsOne(c => c.Phone, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20);

            phone.Property(p => p.CountryCode)
                .HasColumnName("PhoneCountryCode")
                .HasMaxLength(5);
        });

        builder.Property(c => c.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.LoyaltyPoints)
            .IsRequired();

        builder.Property(c => c.LifetimeLoyaltyPoints)
            .IsRequired();

        builder.Property(c => c.LoyaltyTier)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.TotalOrders)
            .IsRequired()
            .HasDefaultValue(0);

        // Navigation: Addresses (owned collection)
        builder.HasMany(c => c.Addresses)
            .WithOne()
            .HasForeignKey("CustomerId")
            .OnDelete(DeleteBehavior.Cascade);

        // Navigation: PaymentMethods (owned collection)
        builder.HasMany(c => c.PaymentMethods)
            .WithOne()
            .HasForeignKey("CustomerId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common queries
        builder.HasIndex(c => c.IsActive)
            .HasDatabaseName("IX_Customers_IsActive");

        builder.HasIndex(c => c.LoyaltyTier)
            .HasDatabaseName("IX_Customers_LoyaltyTier");

        builder.HasIndex(c => c.LastOrderDate)
            .HasDatabaseName("IX_Customers_LastOrderDate");
    }
}

/// <summary>
/// EF Core configuration for DeliveryAddress entity.
/// </summary>
public class DeliveryAddressConfiguration : IEntityTypeConfiguration<DeliveryAddress>
{
    public void Configure(EntityTypeBuilder<DeliveryAddress> builder)
    {
        builder.ToTable("CustomerAddresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Label)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Street)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.BuildingNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Floor)
            .HasMaxLength(10);

        builder.Property(a => a.Apartment)
            .HasMaxLength(20);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.District)
            .HasMaxLength(100);

        builder.Property(a => a.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);

        // Value object: GeoCoordinate (optional)
        builder.OwnsOne(a => a.Coordinates, coord =>
        {
            coord.Property(c => c.Latitude)
                .HasColumnName("Latitude")
                .HasPrecision(9, 6);

            coord.Property(c => c.Longitude)
                .HasColumnName("Longitude")
                .HasPrecision(9, 6);
        });

        builder.Property(a => a.DeliveryInstructions)
            .HasMaxLength(500);

        builder.Property(a => a.IsDefault)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // Index for finding default address
        builder.HasIndex("CustomerId", nameof(DeliveryAddress.IsDefault), nameof(DeliveryAddress.IsActive))
            .HasDatabaseName("IX_CustomerAddresses_Customer_Default_Active");
    }
}

/// <summary>
/// EF Core configuration for PaymentMethod entity.
/// </summary>
public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("CustomerPaymentMethods");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.Label)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Last4Digits)
            .HasMaxLength(4);

        builder.Property(p => p.CardBrand)
            .HasMaxLength(20);

        builder.Property(p => p.ExternalToken)
            .HasMaxLength(500);

        builder.Property(p => p.IsDefault)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Index for finding default payment method
        builder.HasIndex("CustomerId", nameof(PaymentMethod.IsDefault), nameof(PaymentMethod.IsActive))
            .HasDatabaseName("IX_CustomerPaymentMethods_Customer_Default_Active");
    }
}
