namespace KnowledgeVault.Api.Domain;

public class OutboxEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventType { get; set; } = string.Empty;   // e.g. "KnowledgeVault.Api.Events.Note.NoteCreatedEvent"
    public string Payload { get; set; } = string.Empty;     // JSON
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; } = null;
    public int Attempts { get; set; } = 0;
}