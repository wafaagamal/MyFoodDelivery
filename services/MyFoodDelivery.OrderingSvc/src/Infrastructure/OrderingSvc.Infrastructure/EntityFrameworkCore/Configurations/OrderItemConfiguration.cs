using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderingSvc.Domain.Orders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace OrderingSvc.Infrastructure.EntityFrameworkCore.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.ConfigureByConvention();

        builder.Property(i => i.MenuItemId)
            .IsRequired();

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(i => i.SpecialInstructions)
            .HasMaxLength(500);

        // TotalPrice is calculated, so we ignore it for persistence
        builder.Ignore(i => i.TotalPrice);

        builder.HasIndex(i => i.MenuItemId);
    }
}
