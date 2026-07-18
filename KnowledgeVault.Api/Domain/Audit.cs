namespace KnowledgeVault.Api.Domain;

public class Audit
{
    public int Id { get; set; }
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid? NoteId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Data { get; set; } = string.Empty;
}