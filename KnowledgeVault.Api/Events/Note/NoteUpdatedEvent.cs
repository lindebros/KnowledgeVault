namespace KnowledgeVault.Api.Events.Note;

public class NoteUpdatedEvent : DomainEvent
{
    public Guid NoteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}