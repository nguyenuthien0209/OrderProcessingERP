using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Domain.Entities;

namespace Shipping.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.OrderId).IsRequired();
        builder.Property(s => s.Carrier).HasMaxLength(128).IsRequired();
        builder.Property(s => s.TrackingNumber).HasMaxLength(128).IsRequired();
        builder.Property(s => s.ShippedOnUtc).IsRequired();

        builder.HasIndex(s => s.OrderId).IsUnique();
    }
}
