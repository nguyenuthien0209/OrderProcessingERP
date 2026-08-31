using Common.Outbox;
using Microsoft.EntityFrameworkCore;
using Shipping.Domain.Entities;

namespace Shipping.Application.Common.Interfaces;

public interface IShippingDbContext : IOutboxDbContext
{
    DbSet<Shipment> Shipments { get; }
}
