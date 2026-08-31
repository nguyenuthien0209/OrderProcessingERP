using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Common.Outbox;

/// <summary>
/// Fetches, deserializes, publishes, and marks one batch of pending outbox rows. Deliberately has no
/// scheduling or hosting concerns — those belong to <see cref="OutboxProcessor{TDbContext}"/> — so this
/// class can be exercised directly in a test with an in-memory DbContext and a fake IPublishEndpoint.
/// </summary>
public class OutboxDispatcher<TDbContext> where TDbContext : DbContext, IOutboxDbContext
{
    private const int BatchSize = 20;

    private readonly TDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger _logger;

    public OutboxDispatcher(TDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <returns>How many pending rows were found (published or not) in this batch.</returns>
    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        var messages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return 0;

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type)
                    ?? throw new InvalidOperationException($"Unknown integration event type '{message.Type}'.");

                var integrationEvent = JsonSerializer.Deserialize(message.Content, eventType)
                    ?? throw new InvalidOperationException("Failed to deserialize outbox message payload.");

                await _publishEndpoint.Publish(integrationEvent, eventType, cancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogError(ex, "Failed to publish outbox message {MessageId} ({MessageType})", message.Id, message.Type);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return messages.Count;
    }
}
