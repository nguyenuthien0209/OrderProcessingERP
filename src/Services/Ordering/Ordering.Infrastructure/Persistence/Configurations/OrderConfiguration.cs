using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(o => o.CreatedOnUtc).IsRequired();
        builder.Property(o => o.CancellationReason).HasMaxLength(512);
        builder.Property(o => o.Carrier).HasMaxLength(128);
        builder.Property(o => o.TrackingNumber).HasMaxLength(128);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(o => o.TotalAmount);
    }
}
