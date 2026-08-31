using Common.Outbox;
using Microsoft.EntityFrameworkCore;
using Payments.Application.Common.Interfaces;
using Payments.Domain.Entities;

namespace Payments.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext, IPaymentsDbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PendingOrder> PendingOrders => Set<PendingOrder>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentsDbContext).Assembly);
    }
}
