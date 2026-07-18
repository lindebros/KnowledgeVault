using KnowledgeVault.Api.Events.Note;

namespace KnowledgeVault.Api.Events.Handlers;

public class NoteCreatedEventHandler(ILogger<NoteCreatedEventHandler> logger) : IEventHandler<NoteCreatedEvent>
{
    public Task HandleAsync(NoteCreatedEvent @event)
    {
        logger.LogInformation("NoteCreated: {NoteId} Title={Title}", @event.NoteId, @event.Title);
        return Task.CompletedTask;
    }
}