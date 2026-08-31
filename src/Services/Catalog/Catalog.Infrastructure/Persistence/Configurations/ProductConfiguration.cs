using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    // Demo seed data — these ids must match Inventory.Infrastructure's seeded stock items
    // so an order placed through the API resolves to both a price and real stock.
    public static readonly Guid WirelessMouseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid MechanicalKeyboardId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid UsbCDockId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(256).IsRequired();
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.IsActive).IsRequired();

        builder.HasData(
            new { Id = WirelessMouseId, Name = "Wireless Mouse", Price = 24.99m, IsActive = true },
            new { Id = MechanicalKeyboardId, Name = "Mechanical Keyboard", Price = 89.50m, IsActive = true },
            new { Id = UsbCDockId, Name = "USB-C Dock", Price = 129.00m, IsActive = true });
    }
}
