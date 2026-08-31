using Microsoft.EntityFrameworkCore;

namespace Common.Outbox;

/// <summary>Implemented by every service's write DbContext so the generic outbox processor can run against it.</summary>
public interface IOutboxDbContext
{
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
