using System.Text.Json;
using KnowledgeVault.Api.Contracts.Persistence;
using KnowledgeVault.Api.Events.Note;
using KnowledgeVault.Api.Domain;
using KnowledgeVault.Api.Services;
using Microsoft.Extensions.Caching.Memory;

namespace KnowledgeVault.Api.Events.Handlers;

public class NoteDeletedEventHandler : IEventHandler<NoteDeletedEvent>
{
    readonly ILogger<NoteDeletedEventHandler> _logger;
    readonly IMemoryCache _cache;
    readonly INotificationService _notifier;
    readonly AppDbContext _db;

    public NoteDeletedEventHandler(ILogger<NoteDeletedEventHandler> logger,
        IMemoryCache cache,
        INotificationService notifier,
        AppDbContext db)
    {
        _logger = logger;
        _cache = cache;
        _notifier = notifier;
        _db = db;
    }

    public async Task HandleAsync(NoteDeletedEvent @event)
    {
        _logger.LogInformation("NoteDeleted: {NoteId}", @event.NoteId);

        _cache.Remove("notes:all");
        _cache.Remove($"note:{@event.NoteId}");

        await _notifier.NotifyAsync($"Note deleted: {@event.NoteId}");

        var audit = new Audit
        {
            EventId = @event.Id,
            EventType = nameof(NoteDeletedEvent),
            NoteId = @event.NoteId,
            OccurredAt = @event.OccurredAt,
            Data = JsonSerializer.Serialize(new { DeletedAt = @event.DeletedAt })
        };
        _db.Audits.Add(audit);
        await _db.SaveChangesAsync();
    }
}