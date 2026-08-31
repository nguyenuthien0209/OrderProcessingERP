using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("StockReservations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.OrderId).IsRequired();
        builder.Property(r => r.ProductId).IsRequired();
        builder.Property(r => r.Quantity).IsRequired();
        builder.Property(r => r.ReservedOnUtc).IsRequired();

        builder.HasIndex(r => r.OrderId);
    }
}
