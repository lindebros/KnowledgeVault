using KnowledgeVault.Api.Domain;

namespace KnowledgeVault.Api.Repositories;

public class InMemoryNoteRepository : INoteRepository
{
    private readonly List<Note> _notes = [];

    public Task<IEnumerable<Note>> GetAllAsync()
    {
        return Task.FromResult(_notes.AsEnumerable());
    }

    public Task<Note?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(
            _notes.FirstOrDefault(n => n.Id == id));
    }

    public Task<Note> CreateAsync(Note note)
    {
        _notes.Add(note);
        return Task.FromResult(note);
    }

    public Task UpdateAsync(Note note)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        var note = _notes.FirstOrDefault(n => n.Id == id);

        if (note != null)
            _notes.Remove(note);

        return Task.CompletedTask;
    }
}