namespace KnowledgeVault.Api.Domain;

public class NoteTag
{
    public Guid NoteId { get; set; }
    public Note Note { get; set; } = null!;
    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}