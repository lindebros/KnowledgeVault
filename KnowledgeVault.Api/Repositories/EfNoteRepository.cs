using KnowledgeVault.Api.Domain;
using KnowledgeVault.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeVault.Api.Repositories;

public class EfNoteRepository : INoteRepository
{
    private readonly AppDbContext _db;

    public EfNoteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Note>> GetAllAsync()
    {
        return await _db.Notes.ToListAsync();
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        return await _db.Notes.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Note> CreateAsync(Note note)
    {
        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task UpdateAsync(Note note)
    {
        _db.Notes.Update(note);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var note = await _db.Notes.FindAsync(id);

        if (note != null)
        {
            _db.Notes.Remove(note);
            await _db.SaveChangesAsync();
        }
    }
}