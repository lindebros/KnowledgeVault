using KnowledgeVault.Api.Domain;
using KnowledgeVault.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeVault.Api.Services;

public class NoteService(AppDbContext db)
{
    public async Task<List<Note>> GetAllAsync()
    {
        return await db.Notes.ToListAsync();
    }

    public async Task<Note?> GetByIdAsync(Guid id)
    {
        return await db.Notes.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Note> CreateAsync(string title, string content)
    {
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync();

        return note;
    }
    
    public async Task<Note?> UpdateAsync(Guid id, string title, string content)
    {
        var note = await db.Notes.FindAsync(id);

        if (note == null)
            return null;

        note.Title = title;
        note.Content = content;
        note.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return note;
    }

    public async Task DeleteAsync(Guid id)
    {
        var note = await db.Notes.FindAsync(id);
        if (note != null)
        {
            db.Notes.Remove(note);
            await db.SaveChangesAsync();
        }
    }
}