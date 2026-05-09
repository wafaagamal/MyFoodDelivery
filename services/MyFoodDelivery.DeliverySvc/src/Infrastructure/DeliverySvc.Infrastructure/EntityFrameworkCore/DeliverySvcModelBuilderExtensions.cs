using DeliverySvc.Domain.DeliveryTasks;
using DeliverySvc.Domain.Riders;
using Microsoft.EntityFrameworkCore;

namespace DeliverySvc.Infrastructure.EntityFrameworkCore;

public static class DeliverySvcModelBuilderExtensions
{
    public static void ConfigureDeliverySvc(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rider>(b =>
        {
            b.ToTable("Riders");
            b.HasKey(x => x.Id);
            // Id == IdentityUser.Id — no identity columns here (name/email live in AuthSvc)
            b.Property(x => x.VehicleType).HasMaxLength(50);
            b.Property(x => x.VehiclePlate).HasMaxLength(20);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
            b.Property(x => x.IsOnline).IsRequired();
            b.Property(x => x.Latitude).HasColumnName("Latitude");
            b.Property(x => x.Longitude).HasColumnName("Longitude");
            b.Property(x => x.TotalDeliveries).IsRequired();
            b.Property(x => x.TotalEarnings).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(x => x.AverageRating).IsRequired();
            b.Property(x => x.RatingCount).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.IsOnline);
        });

        modelBuilder.Entity<DeliveryTask>(b =>
        {
            b.ToTable("DeliveryTasks");
            b.HasKey(x => x.Id);
            b.Property(x => x.PickupAddress).HasMaxLength(500).IsRequired();
            b.Property(x => x.DeliveryAddress).HasMaxLength(500).IsRequired();
            b.Property(x => x.DeliveryInstructions).HasMaxLength(500);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            b.Property(x => x.FailureReason).HasMaxLength(500);
            b.HasIndex(x => x.OrderId).IsUnique();
            b.HasIndex(x => x.RiderId);
            b.HasIndex(x => x.Status);
        });
    }
}
