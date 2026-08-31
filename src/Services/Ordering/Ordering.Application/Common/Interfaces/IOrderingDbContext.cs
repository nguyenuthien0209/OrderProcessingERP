using Common.Outbox;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Entities;

namespace Ordering.Application.Common.Interfaces;

public interface IOrderingDbContext : IOutboxDbContext
{
    DbSet<Order> Orders { get; }
}
