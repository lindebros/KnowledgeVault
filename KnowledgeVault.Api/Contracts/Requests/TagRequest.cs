using System.ComponentModel.DataAnnotations;

namespace KnowledgeVault.Api.Contracts.Requests;

public class TagRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}