using System.Text.Json;
using KnowledgeVault.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace KnowledgeVault.Api.Events;

public class OutboxPublisher : BackgroundService
{
    readonly IServiceProvider _provider;
    readonly ILogger<OutboxPublisher> _logger;
    private readonly OutboxOptions _opts;

    public OutboxPublisher(IServiceProvider provider, ILogger<OutboxPublisher> logger, IOptions<OutboxOptions> opts)
    {
        
        _provider = provider;
        _logger = logger;
        _opts = opts.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(_opts.IntervalSeconds);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("OutboxPublisher running at: {time}", DateTimeOffset.Now);
                using var scope = _provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var bus = scope.ServiceProvider.GetRequiredService<IEventBus>();

                // pick a batch of unpublished events
                var batch = await db.OutboxEvents
                    .Where(x => x.PublishedAt == null)
                    .OrderBy(x => x.OccurredAt)
                    .Take(_opts.BatchSize)
                    .ToListAsync(stoppingToken);

                _logger.LogInformation("Outbox published {OutboxCount} outbox events", batch.Count);
                
                foreach (var ev in batch)
                {
                    try
                    {
                        _logger.LogInformation("Publishing outbox event {OutboxId} of type {EventType}", ev.Id, ev.EventType);
                        // instantiate event by type
                        var eventType = Type.GetType(ev.EventType!);
                        var domainEvent = (DomainEvent?)JsonSerializer.Deserialize(ev.Payload, eventType!);
                        if (domainEvent == null) { ev.Attempts++; continue; }

                        await bus.PublishAsync((dynamic)domainEvent);
                        ev.PublishedAt = DateTime.UtcNow;
                        ev.Attempts++;
                        db.Update(ev);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        ev.Attempts++;
                        db.Update(ev);
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogError(ex, "Failed publishing outbox event {OutboxId}", ev.Id);
                        // continue to next event; backoff layer handled by interval or attempts logic
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisher top-level error");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}