using System.ComponentModel.DataAnnotations;

namespace KnowledgeVault.Api.Contracts.Requests;

public class CreateNoteRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}