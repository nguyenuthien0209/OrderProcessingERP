using Common.Outbox;
using Microsoft.EntityFrameworkCore;
using Shipping.Application.Common.Interfaces;
using Shipping.Domain.Entities;

namespace Shipping.Infrastructure.Persistence;

public class ShippingDbContext : DbContext, IShippingDbContext
{
    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options)
    {
    }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShippingDbContext).Assembly);
    }
}
