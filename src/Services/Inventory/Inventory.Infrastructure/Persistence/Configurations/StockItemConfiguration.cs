using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.Persistence.Configurations;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    // Demo seed data — the product ids must match Catalog.Infrastructure's seeded products
    // so an order placed against Catalog resolves to real stock here.
    public static readonly Guid WirelessMouseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid MechanicalKeyboardId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid UsbCDockId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProductId).IsRequired();
        builder.Property(s => s.QuantityOnHand).IsRequired();
        builder.Property(s => s.QuantityReserved).IsRequired();
        builder.Ignore(s => s.QuantityAvailable);

        builder.HasIndex(s => s.ProductId).IsUnique();

        builder.HasData(
            new { Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), ProductId = WirelessMouseId, QuantityOnHand = 100, QuantityReserved = 0 },
            new { Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), ProductId = MechanicalKeyboardId, QuantityOnHand = 50, QuantityReserved = 0 },
            new { Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), ProductId = UsbCDockId, QuantityOnHand = 25, QuantityReserved = 0 });
    }
}
