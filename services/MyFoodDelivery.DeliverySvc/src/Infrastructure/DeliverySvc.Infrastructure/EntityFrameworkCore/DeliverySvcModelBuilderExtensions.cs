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
            b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            b.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
            b.Property(x => x.Email).HasMaxLength(200);
            b.Property(x => x.VehicleType).HasMaxLength(50);
            b.Property(x => x.VehiclePlate).HasMaxLength(20);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(50);
            b.Property(x => x.Rating).HasPrecision(3, 2);
            b.HasIndex(x => x.PhoneNumber).IsUnique();
            b.HasIndex(x => x.Status);
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
