namespace Common.Outbox;

/// <summary>
/// Persisted alongside the aggregate change in the same DB transaction (the "transactional outbox"
/// pattern), so that saving state and recording the intent to publish an integration event are atomic.
/// A background <see cref="OutboxProcessor{TDbContext}"/> then delivers it to the bus at-least-once.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
