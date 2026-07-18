using KnowledgeVault.Api.Events.Note;

namespace KnowledgeVault.Api.Events.Handlers;

public class NoteUpdatedEventHandler(ILogger<NoteUpdatedEventHandler> logger) : IEventHandler<NoteUpdatedEvent>
{
    public Task HandleAsync(NoteUpdatedEvent @event)
    {
        logger.LogInformation("NoteUpdated: {NoteId} Title={Title}", @event.NoteId, @event.Title);
        return Task.CompletedTask;
    }
}