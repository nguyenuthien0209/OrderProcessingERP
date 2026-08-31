using Common.Outbox;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;

namespace Payments.Application.Common.Interfaces;

public interface IPaymentsDbContext : IOutboxDbContext
{
    DbSet<Payment> Payments { get; }
    DbSet<PendingOrder> PendingOrders { get; }
}
