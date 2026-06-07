using KnowledgeVault.Api.Domain;

namespace KnowledgeVault.Api.Repositories;

public interface INoteRepository
{
    Task<IEnumerable<Note>> GetAllAsync();

    Task<Note?> GetByIdAsync(Guid id);

    Task<Note> CreateAsync(Note note);

    Task UpdateAsync(Note note);

    Task DeleteAsync(Guid id);
}