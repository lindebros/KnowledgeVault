using System.Text.Json;
using KnowledgeVault.Api.Contracts.Persistence;
using KnowledgeVault.Api.Events.Note;
using KnowledgeVault.Api.Domain;
using KnowledgeVault.Api.Services;
using Microsoft.Extensions.Caching.Memory;

namespace KnowledgeVault.Api.Events.Handlers;

public class NoteUpdatedEventHandler : IEventHandler<NoteUpdatedEvent>
{
    readonly ILogger<NoteUpdatedEventHandler> _logger;
    readonly IMemoryCache _cache;
    readonly INotificationService _notifier;
    readonly AppDbContext _db;

    public NoteUpdatedEventHandler(ILogger<NoteUpdatedEventHandler> logger,
        IMemoryCache cache,
        INotificationService notifier,
        AppDbContext db)
    {
        _logger = logger;
        _cache = cache;
        _notifier = notifier;
        _db = db;
    }

    public async Task HandleAsync(NoteUpdatedEvent @event)
    {
        _logger.LogInformation("NoteUpdated: {NoteId} Title={Title}", @event.NoteId, @event.Title);

        _cache.Remove("notes:all");
        _cache.Remove($"note:{@event.NoteId}");

        await _notifier.NotifyAsync($"Note updated: {@event.Title}");

        var audit = new Audit
        {
            EventId = @event.Id,
            EventType = nameof(NoteUpdatedEvent),
            NoteId = @event.NoteId,
            OccurredAt = @event.OccurredAt,
            Data = JsonSerializer.Serialize(new { @event.Title, @event.Content })
        };
        _db.Audits.Add(audit);
        await _db.SaveChangesAsync();
    }
}