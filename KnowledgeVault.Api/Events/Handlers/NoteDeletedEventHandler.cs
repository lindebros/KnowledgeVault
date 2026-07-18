using KnowledgeVault.Api.Events.Note;

namespace KnowledgeVault.Api.Events.Handlers;

public class NoteDeletedEventHandler : IEventHandler<NoteDeletedEvent>
{
readonly ILogger<NoteDeletedEventHandler> _logger;
public NoteDeletedEventHandler(ILogger<NoteDeletedEventHandler> logger) => _logger = logger;
public Task HandleAsync(NoteDeletedEvent @event)
{
    _logger.LogInformation("NoteDeleted: {NoteId}", @event.NoteId);
    return Task.CompletedTask;
}
}