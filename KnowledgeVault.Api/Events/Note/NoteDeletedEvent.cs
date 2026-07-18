namespace KnowledgeVault.Api.Events.Note;

public class NoteDeletedEvent : DomainEvent
{
    public Guid NoteId { get; set; }
    public DateTime DeletedAt { get; set; }
}