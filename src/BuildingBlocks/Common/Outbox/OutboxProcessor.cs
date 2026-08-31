using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Outbox;

/// <summary>
/// Scheduling only: owns the poll loop, per-tick DI scope, and shutdown handling, then hands the actual
/// work to <see cref="OutboxDispatcher{TDbContext}"/>. One instance is registered per service, parameterized
/// by that service's own <see cref="DbContext"/> type. Delivery is at-least-once — consumers must be idempotent.
/// </summary>
public class OutboxProcessor<TDbContext> : BackgroundService
    where TDbContext : DbContext, IOutboxDbContext
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor<TDbContext>> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor<TDbContext>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
                var dispatcher = new OutboxDispatcher<TDbContext>(dbContext, publishEndpoint, _logger);

                await dispatcher.DispatchPendingAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Outbox processing failed for {DbContext}", typeof(TDbContext).Name);
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
        }
    }
}
