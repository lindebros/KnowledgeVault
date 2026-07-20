using System.Text.Json;
using KnowledgeVault.Api.Contracts.Persistence;
using KnowledgeVault.Api.Events.Note;
using KnowledgeVault.Api.Domain;
using KnowledgeVault.Api.Services;
using Microsoft.Extensions.Caching.Memory;

namespace KnowledgeVault.Api.Events.Handlers;

public class NoteCreatedEventHandler : IEventHandler<NoteCreatedEvent>
{
    readonly ILogger<NoteCreatedEventHandler> _logger;
    readonly IMemoryCache _cache;
    readonly INotificationService _notifier;
    readonly AppDbContext _db;

    public NoteCreatedEventHandler(ILogger<NoteCreatedEventHandler> logger,
        IMemoryCache cache,
        INotificationService notifier,
        AppDbContext db)
    {
        _logger = logger;
        _cache = cache;
        _notifier = notifier;
        _db = db;
    }

    public async Task HandleAsync(NoteCreatedEvent @event)
    {
        _logger.LogInformation("NoteCreated: {NoteId} Title={Title}", @event.NoteId, @event.Title);

        // Invalidate caches
        _cache.Remove("notes:all");
        _cache.Remove($"note:{@event.NoteId}");

        // Notify
        await _notifier.NotifyAsync($"Note created: {@event.Title}");

        // Audit
        var audit = new Audit
        {
            EventId = @event.Id,
            EventType = nameof(NoteCreatedEvent),
            NoteId = @event.NoteId,
            OccurredAt = @event.OccurredAt,
            Data = JsonSerializer.Serialize(new { @event.Title, @event.Content })
        };
        _db.Audits.Add(audit);
        await _db.SaveChangesAsync();
    }
}