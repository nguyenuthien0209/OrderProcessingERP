using Common.Outbox;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Common.Interfaces;

public interface IInventoryDbContext : IOutboxDbContext
{
    DbSet<StockItem> StockItems { get; }
    DbSet<StockReservation> StockReservations { get; }
}
