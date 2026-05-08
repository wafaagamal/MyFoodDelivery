using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingSvc.Domain.Orders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace OrderingSvc.Infrastructure.EntityFrameworkCore.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.ConfigureByConvention();

        builder.Property(o => o.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.RestaurantId)
            .IsRequired();

        builder.Property(o => o.RiderId);

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.SpecialInstructions)
            .HasMaxLength(500);

        builder.Property(o => o.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(o => o.DeliveryFee)
            .HasPrecision(18, 2);

        builder.Property(o => o.ServiceFee)
            .HasPrecision(18, 2);

        builder.Property(o => o.Discount)
            .HasPrecision(18, 2);

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.CancellationReason)
            .HasMaxLength(500);

        // Owned type for DeliveryAddress (value object)
        builder.OwnsOne(o => o.DeliveryAddress, da =>
        {
            da.Property(a => a.Street)
                .HasColumnName("DeliveryStreet")
                .IsRequired()
                .HasMaxLength(200);

            da.Property(a => a.BuildingNumber)
                .HasColumnName("DeliveryBuildingNumber")
                .IsRequired()
                .HasMaxLength(50);

            da.Property(a => a.Floor)
                .HasColumnName("DeliveryFloor")
                .HasMaxLength(10);

            da.Property(a => a.Apartment)
                .HasColumnName("DeliveryApartment")
                .HasMaxLength(20);

            da.Property(a => a.City)
                .HasColumnName("DeliveryCity")
                .IsRequired()
                .HasMaxLength(100);

            da.Property(a => a.PostalCode)
                .HasColumnName("DeliveryPostalCode")
                .IsRequired()
                .HasMaxLength(20);

            da.Property(a => a.Country)
                .HasColumnName("DeliveryCountry")
                .IsRequired()
                .HasMaxLength(100);

            da.Property(a => a.Latitude)
                .HasColumnName("DeliveryLatitude");

            da.Property(a => a.Longitude)
                .HasColumnName("DeliveryLongitude");

            da.Property(a => a.DeliveryInstructions)
                .HasColumnName("DeliveryInstructions")
                .HasMaxLength(500);
        });

        // Navigation to OrderItems
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.RestaurantId);
        builder.HasIndex(o => o.RiderId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreationTime);
    }
}
